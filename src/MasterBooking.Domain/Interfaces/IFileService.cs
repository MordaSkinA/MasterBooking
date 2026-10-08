using System.IO;
using System.Threading.Tasks;

namespace MasterBooking.Domain.Interfaces
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string extension, long fileSize);
    }
}
