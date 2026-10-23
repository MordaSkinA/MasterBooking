using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Web.Helpers;

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

        public TimeZoneInfo Zone { get; set; } = TimeZoneInfo.Utc;

        public int WeeklyAppointmentCount { get; set; }
        public List<Appointment> WeeklyAppointments { get; set; } = new();

        public async Task OnGetAsync()
        {
            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var masterTimeZone = MasterTime.Zone(settings?.TimeZoneId);
            Zone = masterTimeZone;

            var masterNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, masterTimeZone);
            var masterToday = masterNow.Date;

            var startOfTodayUtc = TimeZoneInfo.ConvertTimeToUtc(masterToday, masterTimeZone);
            var startOfNextDayUtc = TimeZoneInfo.ConvertTimeToUtc(masterToday.AddDays(1), masterTimeZone);

            var startOfMonthInMaster = new DateTime(masterToday.Year, masterToday.Month, 1);
            var startOfMonthUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonthInMaster, masterTimeZone);
            var endOfMonthUtc = TimeZoneInfo.ConvertTimeToUtc(startOfMonthInMaster.AddMonths(1), masterTimeZone);


            TodayAppointmentsCount = await _context.Appointments
                .CountAsync(a => a.StartDateTime >= startOfTodayUtc && a.StartDateTime < startOfNextDayUtc);


            var stats = await _financeService.GetStatisticsAsync(startOfMonthUtc, endOfMonthUtc);
            MonthlyRevenue = stats.TotalIncome;
            AverageCheck = stats.AverageCheck;


            UpcomingAppointments = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.StartDateTime > DateTime.UtcNow)
                .OrderBy(a => a.StartDateTime)
                .Take(5)
                .ToListAsync();


            int diff = (7 + (masterToday.DayOfWeek - DayOfWeek.Monday)) % 7;
            var startOfWeekInMaster = masterToday.AddDays(-diff);
            var endOfWeekInMaster = startOfWeekInMaster.AddDays(7);

            var startOfWeekUtc = TimeZoneInfo.ConvertTimeToUtc(startOfWeekInMaster, masterTimeZone);
            var endOfWeekUtc = TimeZoneInfo.ConvertTimeToUtc(endOfWeekInMaster, masterTimeZone);

            WeeklyAppointments = await _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => a.StartDateTime >= startOfWeekUtc && a.StartDateTime < endOfWeekUtc)
                .OrderBy(a => a.StartDateTime)
                .ToListAsync();

            WeeklyAppointmentCount = WeeklyAppointments.Count;
        }
    }
}
