using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Infrastructure.Services;

namespace MasterBooking.Web.Pages.Admin.Settings
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IMasterProvider _masterProvider;
        private readonly IFileService _fileService;

        public IndexModel(ApplicationDbContext context, IMasterProvider masterProvider, IFileService fileService)
        {
            _context = context;
            _masterProvider = masterProvider;
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
            var masterId = _masterProvider.GetMasterId();
            if (masterId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Settings = await _context.SiteSettings
                .FirstOrDefaultAsync(s => s.MasterId == masterId.Value)
                ?? new SiteSettings { MasterId = masterId.Value };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var masterId = _masterProvider.GetMasterId();
            if (masterId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var existingSettings = await _context.SiteSettings
                .FirstOrDefaultAsync(s => s.MasterId == masterId.Value);

            if (existingSettings == null)
            {
                existingSettings = new SiteSettings { MasterId = masterId.Value };
                _context.SiteSettings.Add(existingSettings);
            }

            existingSettings.Theme = Settings.Theme;
            existingSettings.PrimaryColor = Settings.PrimaryColor;
            existingSettings.AboutMe = Settings.AboutMe;
            existingSettings.PhoneNumber = Settings.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(Settings.SocialLinks))
            {
                if (!Uri.TryCreate(Settings.SocialLinks, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    ModelState.AddModelError("Settings.SocialLinks", "Введите корректный URL (http:// или https://)");
                    return Page();
                }
            }

            existingSettings.SocialLinks = Settings.SocialLinks;
            existingSettings.ShowCalendar = Settings.ShowCalendar;

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
