using Microsoft.AspNetCore.Hosting;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Domain.Interfaces;
using SkiaSharp;

namespace MasterBooking.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string UploadFolder = "uploads";
        private const long MaxFileSize = 5242880;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string extension, long fileSize)
        {
            if (fileSize > MaxFileSize)
            {
                throw new FileServiceException("Файл слишком большой. Максимальный размер — 5 МБ.");
            }

            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension.ToLower()))
            {
                throw new FileServiceException("Недопустимый тип файла. Разрешены только изображения (JPG, PNG, WebP).");
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, UploadFolder);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension.ToLower()}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            try
            {
                using (var bitmap = SKBitmap.Decode(fileStream))
                {
                    if (bitmap == null)
                    {
                        throw new FileServiceException("Не удалось декодировать изображение.");
                    }

                    int newWidth = bitmap.Width;
                    int newHeight = bitmap.Height;

                    if (newWidth > 1920 || newHeight > 1080)
                    {
                        float ratio = Math.Min(1920f / newWidth, 1080f / newHeight);
                        newWidth = (int)(newWidth * ratio);
                        newHeight = (int)(newHeight * ratio);
                    }

                    using (var resizedBitmap = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High))
                    {
                        SKEncodedImageFormat format = extension.ToLower() switch
                        {
                            ".png" => SKEncodedImageFormat.Png,
                            ".webp" => SKEncodedImageFormat.Webp,
                            _ => SKEncodedImageFormat.Jpeg
                        };

                        using (var image = SKImage.FromBitmap(resizedBitmap))
                        using (var data = image.Encode(format, 85))
                        {
                            using (var stream = File.OpenWrite(filePath))
                            {
                                await data.AsStream().CopyToAsync(stream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not FileServiceException)
            {
                throw new FileServiceException($"Ошибка при обработке изображения: {ex.Message}");
            }

            return Path.Combine("/", UploadFolder, uniqueFileName).Replace("\\", "/");
        }
    }
}
