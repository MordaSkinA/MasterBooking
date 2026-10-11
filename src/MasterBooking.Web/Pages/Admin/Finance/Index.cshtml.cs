using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages.Admin.Finance
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IFinanceService _financeService;
        private readonly ApplicationDbContext _context;

        public IndexModel(IFinanceService financeService, ApplicationDbContext context)
        {
            _financeService = financeService;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
        }

        public FinanceStatistics? Statistics { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Set defaults if not provided
            if (Input.StartDate == default)
            {
                var settings = await _context.SiteSettings.FirstOrDefaultAsync();
                var timeZoneId = settings?.TimeZoneId ?? "Russian Standard Time";
                var masterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                var masterNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, masterTimeZone);

                Input.StartDate = masterNow.AddMonths(-1).Date;
                Input.EndDate = masterNow.Date.AddDays(1);
            }

            await LoadStatistics();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await LoadStatistics();
            return Page();
        }

        private async Task LoadStatistics()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var timeZoneId = settings?.TimeZoneId ?? "Russian Standard Time";
            var masterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var startDateUtc = TimeZoneInfo.ConvertTimeToUtc(Input.StartDate, masterTimeZone);
            var endDateUtc = TimeZoneInfo.ConvertTimeToUtc(Input.EndDate, masterTimeZone);

            Statistics = await _financeService.GetStatisticsAsync(startDateUtc, endDateUtc);
        }
    }
}
