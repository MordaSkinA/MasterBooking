using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Settings
{
    [Authorize]
    public class IndexModel : PageModel
    {

        public static readonly DayOfWeek[] Days =
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
            DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
        };

        public static readonly string[] DayNames = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;

        public IndexModel(ApplicationDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public class DayInput
        {
            public bool Enabled { get; set; }
            public TimeOnly? Start { get; set; }
            public TimeOnly? End { get; set; }
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Укажите имя мастера")]
            [StringLength(200, ErrorMessage = "Не длиннее 200 символов")]
            public string DisplayName { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "Не длиннее 100 символов")]
            public string? City { get; set; }

            public string? AboutMe { get; set; }

            [StringLength(50, ErrorMessage = "Не длиннее 50 символов")]
            public string? PhoneNumber { get; set; }

            [StringLength(200, ErrorMessage = "Не длиннее 200 символов")]
            public string? SocialLinks { get; set; }

            [StringLength(200, ErrorMessage = "Не длиннее 200 символов")]
            public string? Address { get; set; }

            public bool IsPublished { get; set; } = true;

            public List<DayInput> WorkDays { get; set; } = new();
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public IFormFile? CoverImageFile { get; set; }

        public string CoverImage { get; set; } = string.Empty;

        public string SiteAddress => $"{Request.Scheme}://{Request.Host}/";

        public async Task OnGetAsync()
        {
            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var hours = await _context.WorkingHours.AsNoTracking().ToListAsync();

            Input = new InputModel
            {
                DisplayName = settings?.DisplayName ?? string.Empty,
                City = settings?.City,
                AboutMe = settings?.AboutMe,
                PhoneNumber = settings?.PhoneNumber,
                SocialLinks = settings?.SocialLinks,
                Address = settings?.Address,
                IsPublished = settings?.IsPublished ?? true,
                WorkDays = Days.Select(d =>
                {
                    var blocks = hours.Where(h => h.DayOfWeek == d).OrderBy(h => h.StartTime).ToList();
                    return blocks.Count == 0
                        ? new DayInput { Enabled = false, Start = new TimeOnly(10, 0), End = new TimeOnly(19, 0) }
                        : new DayInput
                        {
                            Enabled = true,
                            Start = TimeOnly.FromTimeSpan(blocks.First().StartTime),
                            End = TimeOnly.FromTimeSpan(blocks.Last().EndTime)
                        };
                }).ToList()
            };
            CoverImage = settings?.CoverImage ?? string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            Input.WorkDays ??= new();
            while (Input.WorkDays.Count < Days.Length)
            {
                Input.WorkDays.Add(new DayInput());
            }

            for (var i = 0; i < Days.Length; i++)
            {
                var day = Input.WorkDays[i];
                if (day.Enabled && (day.Start == null || day.End == null || day.Start >= day.End))
                {
                    ModelState.AddModelError(string.Empty, $"{DayNames[i]}: время начала должно быть раньше времени окончания.");
                }
            }

            var existing = await _context.SiteSettings.FirstOrDefaultAsync();
            CoverImage = existing?.CoverImage ?? string.Empty;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (existing == null)
            {
                existing = new SiteSettings();
                _context.SiteSettings.Add(existing);
            }

            existing.DisplayName = Input.DisplayName.Trim();
            existing.City = Input.City?.Trim() ?? string.Empty;
            existing.AboutMe = Input.AboutMe ?? string.Empty;
            existing.PhoneNumber = Input.PhoneNumber?.Trim() ?? string.Empty;
            existing.SocialLinks = Input.SocialLinks?.Trim() ?? string.Empty;
            existing.Address = Input.Address?.Trim() ?? string.Empty;
            existing.IsPublished = Input.IsPublished;

            if (CoverImageFile != null)
            {
                try
                {
                    existing.CoverImage = await _fileService.UploadFileAsync(
                        CoverImageFile.OpenReadStream(),
                        CoverImageFile.FileName,
                        Path.GetExtension(CoverImageFile.FileName),
                        CoverImageFile.Length);
                }
                catch (FileServiceException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return Page();
                }
            }


            var currentHours = await _context.WorkingHours.ToListAsync();
            _context.WorkingHours.RemoveRange(currentHours);
            for (var i = 0; i < Days.Length; i++)
            {
                var day = Input.WorkDays[i];
                if (day.Enabled)
                {
                    _context.WorkingHours.Add(new WorkingHours
                    {
                        DayOfWeek = Days[i],
                        StartTime = day.Start!.Value.ToTimeSpan(),
                        EndTime = day.End!.Value.ToTimeSpan()
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
