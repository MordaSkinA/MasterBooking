using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Web.Helpers;

namespace MasterBooking.Web.Pages.Admin.Clients
{
    [Authorize]
    public class IndexModel : PageModel
    {

        private const int RegularVisits = 3;


        private const int LapsedDays = 60;

        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public record Row(Client Client, int Visits, decimal Total, DateTime? LastVisit);

        public record Visit(DateTime Date, string Service, decimal Price);

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

 
        [BindProperty(SupportsGet = true)]
        public string? Filter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Id { get; set; }

        public List<Row> Rows { get; set; } = new();
        public Row? Selected { get; set; }
        public List<Visit> History { get; set; } = new();
        public TimeZoneInfo Zone { get; set; } = TimeZoneInfo.Utc;

        public async Task OnGetAsync()
        {
            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            Zone = MasterTime.Zone(settings?.TimeZoneId);

            var clientsQuery = _context.Clients.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(Q))
            {
                var q = Q.Trim().ToLower();
                clientsQuery = clientsQuery.Where(c => c.Name.ToLower().Contains(q) || c.PhoneNumber.Contains(q));
            }

            var clients = await clientsQuery.OrderBy(c => c.Name).ToListAsync();

            var stats = (await _context.Appointments.AsNoTracking()
                    .Where(a => a.ClientId != null && (a.Status == "Done" || a.Status == "Completed"))
                    .GroupBy(a => a.ClientId)
                    .Select(g => new { ClientId = g.Key, Visits = g.Count(), Total = g.Sum(a => a.FinalPrice), Last = g.Max(a => a.StartDateTime) })
                    .ToListAsync())
                .ToDictionary(x => x.ClientId!.Value);

            var lapsedBefore = DateTime.UtcNow.AddDays(-LapsedDays);

            Rows = clients
                .Select(c => stats.TryGetValue(c.Id, out var s)
                    ? new Row(c, s.Visits, s.Total, s.Last)
                    : new Row(c, 0, 0m, null))
                .Where(r => Filter switch
                {
                    "regular" => r.Visits >= RegularVisits,
                    "lapsed" => r.LastVisit != null && r.LastVisit < lapsedBefore,
                    _ => true
                })
                .ToList();

            Selected = Rows.FirstOrDefault(r => r.Client.Id == Id) ?? Rows.FirstOrDefault();

            if (Selected != null)
            {
                History = (await _context.Appointments.AsNoTracking()
                        .Where(a => a.ClientId == Selected.Client.Id && (a.Status == "Done" || a.Status == "Completed"))
                        .OrderByDescending(a => a.StartDateTime)
                        .Take(5)
                        .Select(a => new { a.StartDateTime, Service = a.Service.Name, a.FinalPrice })
                        .ToListAsync())
                    .Select(a => new Visit(MasterTime.ToLocal(a.StartDateTime, Zone), a.Service, a.FinalPrice))
                    .ToList();
            }
        }
    }
}
