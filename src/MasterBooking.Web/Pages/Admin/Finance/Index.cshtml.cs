using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Web.Helpers;

namespace MasterBooking.Web.Pages.Admin.Finance
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private static readonly string[] ShortDays = { "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };
        private static readonly string[] ShortMonths = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public record Bar(string Label, decimal Value);

        public record Line(DateTime Date, string Client, string Service, string Payment, decimal Amount);


        [BindProperty(SupportsGet = true)]
        public string? Period { get; set; }

        public string ActivePeriod => Period is "week" or "year" ? Period : "month";

        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit => Income - Expenses;
        public decimal AverageCheck { get; set; }

        public string ChartTitle { get; set; } = "Доход по неделям";
        public List<Bar> Bars { get; set; } = new();
        public List<Line> Lines { get; set; } = new();

        public async Task OnGetAsync()
        {
            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var zone = MasterTime.Zone(settings?.TimeZoneId);
            var today = MasterTime.ToLocal(DateTime.UtcNow, zone).Date;

            DateTime from, to;
            switch (ActivePeriod)
            {
                case "week":
                    from = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
                    to = from.AddDays(7);
                    break;
                case "year":
                    from = new DateTime(today.Year, 1, 1);
                    to = from.AddYears(1);
                    break;
                default:
                    from = new DateTime(today.Year, today.Month, 1);
                    to = from.AddMonths(1);
                    break;
            }

            var fromUtc = MasterTime.ToUtc(from, zone);
            var toUtc = MasterTime.ToUtc(to, zone);

            var done = await _context.Appointments.AsNoTracking()
                .Include(a => a.Client)
                .Include(a => a.Service)
                .Where(a => (a.Status == "Done" || a.Status == "Completed") && a.StartDateTime >= fromUtc && a.StartDateTime < toUtc)
                .OrderByDescending(a => a.StartDateTime)
                .ToListAsync();

            Income = done.Sum(a => a.FinalPrice);
            AverageCheck = done.Count == 0 ? 0 : Income / done.Count;
            Expenses = await _context.Expenses.AsNoTracking()
                .Where(e => e.Date >= fromUtc && e.Date < toUtc)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var local = done.Select(a => (Date: MasterTime.ToLocal(a.StartDateTime, zone), a.FinalPrice)).ToList();

            switch (ActivePeriod)
            {
                case "week":
                    ChartTitle = "Доход по дням";
                    Bars = Enumerable.Range(0, 7)
                        .Select(i => from.AddDays(i))
                        .Select(d => new Bar(ShortDays[(int)d.DayOfWeek] + " " + d.Day, local.Where(x => x.Date.Date == d).Sum(x => x.FinalPrice)))
                        .ToList();
                    break;
                case "year":
                    ChartTitle = "Доход по месяцам";
                    Bars = Enumerable.Range(1, 12)
                        .Select(m => new Bar(ShortMonths[m - 1], local.Where(x => x.Date.Month == m).Sum(x => x.FinalPrice)))
                        .ToList();
                    break;
                default:
                    ChartTitle = "Доход по неделям";
                    var weeks = (DateTime.DaysInMonth(from.Year, from.Month) + 6) / 7;
                    Bars = Enumerable.Range(0, weeks)
                        .Select(w => new Bar("Нед. " + (w + 1), local.Where(x => (x.Date.Day - 1) / 7 == w).Sum(x => x.FinalPrice)))
                        .ToList();
                    break;
            }

            Lines = done.Take(20)
                .Select(a => new Line(
                    MasterTime.ToLocal(a.StartDateTime, zone),
                    a.Client?.Name ?? string.Empty,
                    a.Service?.Name ?? string.Empty,
                    PaymentText(a.PaymentMethod),
                    a.FinalPrice))
                .ToList();
        }

        public static string PaymentText(string? method) => method switch
        {
            "Card" => "Карта",
            "Transfer" => "Перевод",
            _ => "Наличные"
        };
    }
}
