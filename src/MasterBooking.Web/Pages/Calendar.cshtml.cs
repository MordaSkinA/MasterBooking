using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Web.Helpers;

namespace MasterBooking.Web.Pages
{
    public class CalendarModel : PublicPageModelBase
    {

        private const int SlotHours = 2;

        private static readonly string[] ShortDays = { "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };

        public CalendarModel(ApplicationDbContext context) : base(context)
        {
        }


        public record Slot(string Text, bool Busy);

        public record Day(string Title, List<Slot> Slots);

        public List<Day> Week { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadPageDataAsync();
        }

        protected override async Task LoadPageDataAsync()
        {
            await LoadSettingsAsync();

            if (Settings == null)
            {
                return;
            }

            var zone = MasterTime.Zone(Settings.TimeZoneId);
            var nowLocal = MasterTime.ToLocal(DateTime.UtcNow, zone);


            var monday = nowLocal.Date.AddDays(-(((int)nowLocal.DayOfWeek + 6) % 7));
            var weekEndUtc = MasterTime.ToUtc(monday.AddDays(7), zone);
            var weekStartUtc = MasterTime.ToUtc(monday, zone);

            var workingHours = await Context.WorkingHours.AsNoTracking().ToListAsync();
            var daysOff = (await Context.DayOffs.AsNoTracking().ToListAsync()).Select(d => d.Date.Date).ToHashSet();


            var busy = await Context.Appointments.AsNoTracking()
                .Where(a => a.Status != "Cancelled" && a.EndDateTime > weekStartUtc && a.StartDateTime < weekEndUtc)
                .Select(a => new { a.StartDateTime, a.EndDateTime })
                .ToListAsync();

            var days = new List<(DateTime Date, List<DateTime> Starts)>();
            for (var i = 0; i < 7; i++)
            {
                var date = monday.AddDays(i);
                var starts = new List<DateTime>();
                var hours = workingHours.FirstOrDefault(w => w.DayOfWeek == date.DayOfWeek);
                if (hours != null && !daysOff.Contains(date))
                {
                    for (var t = date + hours.StartTime; t + TimeSpan.FromHours(SlotHours) <= date + hours.EndTime; t = t.AddHours(SlotHours))
                    {
                        starts.Add(t);
                    }
                }
                days.Add((date, starts));
            }


            var offCount = Math.Max(days.Max(d => d.Starts.Count), 1);

            foreach (var (date, starts) in days)
            {
                var title = ShortDays[(int)date.DayOfWeek] + " " + date.Day;
                var slots = new List<Slot>();

                if (starts.Count == 0)
                {
                    for (var i = 0; i < offCount; i++)
                    {
                        slots.Add(new Slot("Выходной", true));
                    }
                }
                else
                {
                    foreach (var start in starts)
                    {
                        var startUtc = MasterTime.ToUtc(start, zone);
                        var endUtc = startUtc.AddHours(SlotHours);
                        var isBusy = start < nowLocal
                                     || busy.Any(b => b.StartDateTime < endUtc && b.EndDateTime > startUtc);
                        slots.Add(new Slot(isBusy ? "Занято" : start.ToString("HH:mm"), isBusy));
                    }
                }

                Week.Add(new Day(title, slots));
            }
        }
    }
}
