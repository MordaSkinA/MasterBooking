using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Infrastructure.Services;

namespace MasterBooking.Web.Pages.Admin.Settings
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

        [BindProperty]
        public SiteSettings Settings { get; set; } = default!;

        [BindProperty]
        public IFormFile? CoverImageFile { get; set; }

        [BindProperty]
        public IFormFile? LogoFile { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (Settings == null)
            {
                Settings = new SiteSettings();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingSettings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (existingSettings == null)
            {
                existingSettings = new SiteSettings();
                _context.SiteSettings.Add(existingSettings);
            }

            existingSettings.DisplayName = Settings.DisplayName;
            existingSettings.UserId = Settings.UserId;
            existingSettings.Theme = Settings.Theme;
            existingSettings.PrimaryColor = Settings.PrimaryColor;
            existingSettings.AboutMe = Settings.AboutMe;
            existingSettings.PhoneNumber = Settings.PhoneNumber;
            existingSettings.SocialLinks = Settings.SocialLinks;
            existingSettings.ShowCalendar = Settings.ShowCalendar;
            existingSettings.CoverImage = Settings.CoverImage;
            existingSettings.Logo = Settings.Logo;
            existingSettings.TimeZoneId = Settings.TimeZoneId;

            try
            {
                if (CoverImageFile != null)
                {
                    existingSettings.CoverImage = await _fileService.UploadFileAsync(
                        CoverImageFile.OpenReadStream(),
                        CoverImageFile.FileName,
                        Path.GetExtension(CoverImageFile.FileName),
                        CoverImageFile.Length);
                }
                if (LogoFile != null)
                {
                    existingSettings.Logo = await _fileService.UploadFileAsync(
                        LogoFile.OpenReadStream(),
                        LogoFile.FileName,
                        Path.GetExtension(LogoFile.FileName),
                        LogoFile.Length);
                }
            }
            catch (FileServiceException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
