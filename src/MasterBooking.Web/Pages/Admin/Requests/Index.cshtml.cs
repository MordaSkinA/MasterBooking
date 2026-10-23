using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages.Admin.Requests
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }


        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        public List<ServiceRequest> Requests { get; set; } = new();

        public async Task OnGetAsync()
        {

            var query = _context.ServiceRequests.AsNoTracking()
                .Include(r => r.Service)
                .Where(r => r.Status != ServiceRequest.Rejected);

            query = Status switch
            {
                "new" => query.Where(r => r.Status == ServiceRequest.New),
                "contacted" => query.Where(r => r.Status == ServiceRequest.Contacted),
                "converted" => query.Where(r => r.Status == ServiceRequest.Converted),
                _ => query
            };

            Requests = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<IActionResult> OnPostContactAsync(int id, string? status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request is { Status: ServiceRequest.New })
            {
                request.Status = ServiceRequest.Contacted;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { status });
        }

        public async Task<IActionResult> OnPostConvertAsync(int id, string? status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request != null && request.Status is ServiceRequest.New or ServiceRequest.Contacted)
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == request.Phone);
                if (client == null)
                {
                    var masterId = await _context.SiteSettings.Select(s => s.UserId).FirstOrDefaultAsync() ?? string.Empty;
                    _context.Clients.Add(new Client
                    {
                        Name = request.Name,
                        PhoneNumber = request.Phone,
                        Notes = request.Comment,
                        MasterId = masterId
                    });
                }

                request.Status = ServiceRequest.Converted;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { status });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string? status)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request is { Status: ServiceRequest.New or ServiceRequest.Contacted })
            {
                request.Status = ServiceRequest.Rejected;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { status });
        }
    }
}
