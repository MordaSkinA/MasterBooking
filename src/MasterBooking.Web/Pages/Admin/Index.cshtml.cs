using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages.Admin
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IFinanceService _financeService;

        public IndexModel(ApplicationDbContext context, IFinanceService financeService)
        {
            _context = context;
            _financeService = financeService;
        }

        public int TodayAppointmentsCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal AverageCheck { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; } = new();

        public async Task OnGetAsync()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var timeZoneId = settings?.TimeZoneId ?? "Russian Standard Time";
            var masterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var masterNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, masterTimeZone);
            var masterToday = masterNow.Date;

            var startOfTodayUtc = TimeZoneInfo.ConvertTimeToUtc(masterToday, masterTimeZone);
            var startOfNextDayUtc = TimeZoneInfo.ConvertTimeToUtc(masterToday.AddDays(1), masterTimeZone);

            var startOfMonthInMaster = new DateTime(masterToday.Year, masterToday.Month, 1);
            var startOfMonthUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonthInMaster, masterTimeZone);
            var endOfMonthUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonthInMaster.AddMonths(1), masterTimeZone);

            // 1. Today's appointments
            TodayAppointmentsCount = await _context.Appointments
                .CountAsync(a => a.StartDateTime >= startOfTodayUtc && a.StartDateTime < startOfNextDayUtc);

            // 2. Monthly revenue and average check
            var stats = await _financeService.GetStatisticsAsync(startOfMonthUtc, endOfMonthUtc);
            MonthlyRevenue = stats.TotalIncome;
            AverageCheck = stats.AverageCheck;

            // 3. Upcoming appointments
            UpcomingAppointments = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.StartDateTime > DateTime.UtcNow)
                .OrderBy(a => a.StartDateTime)
                .Take(5)
                .ToListAsync();
        }
    }
}
