using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages.Admin.Showcase
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public IndexModel(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public SiteSettings? Settings { get; set; }
        public List<GalleryImage> Gallery { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();

        [BindProperty]
        public IFormFile? Photo { get; set; }

        public async Task OnGetAsync()
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            Settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            Gallery = await _context.GalleryImages.AsNoTracking().OrderBy(g => g.Order).ThenBy(g => g.Id).ToListAsync();
            Reviews = (await _context.Reviews.AsNoTracking().ToListAsync())
                .OrderBy(r => r.Status == Review.Pending ? 0 : 1)
                .ThenByDescending(r => r.CreatedAt)
                .ToList();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (Photo == null)
            {
                return RedirectToPage();
            }

            try
            {
                var path = await _fileService.UploadFileAsync(
                    Photo.OpenReadStream(),
                    Photo.FileName,
                    Path.GetExtension(Photo.FileName),
                    Photo.Length);

                var nextOrder = (await _context.GalleryImages.MaxAsync(g => (int?)g.Order) ?? -1) + 1;
                _context.GalleryImages.Add(new GalleryImage { ImagePath = path, Order = nextOrder });


                var settings = await _context.SiteSettings.FirstOrDefaultAsync();
                if (settings != null && string.IsNullOrWhiteSpace(settings.CoverImage))
                {
                    settings.CoverImage = path;
                }

                await _context.SaveChangesAsync();
            }
            catch (FileServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemoveAsync(int id)
        {
            var image = await _context.GalleryImages.FindAsync(id);
            if (image != null)
            {
                var settings = await _context.SiteSettings.FirstOrDefaultAsync();
                if (settings != null && settings.CoverImage == image.ImagePath)
                {
                    settings.CoverImage = string.Empty;
                }

                _context.GalleryImages.Remove(image);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostPublishAsync(int id) => await SetReviewStatusAsync(id, Review.Published);

        public async Task<IActionResult> OnPostHideAsync(int id) => await SetReviewStatusAsync(id, Review.Hidden);

        private async Task<IActionResult> SetReviewStatusAsync(int id, string status)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.Status = status;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
