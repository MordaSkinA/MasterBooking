using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages
{
    public class ServicesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ServicesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSettings? Settings { get; set; }
        public List<Service> Services { get; set; } = new();

        public async Task OnGetAsync()
        {
            Settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();

            if (Settings is { IsPublished: false } && User.Identity?.IsAuthenticated != true)
            {
                Settings = null;
            }

            if (Settings != null)
            {
                Services = await _context.Services
                    .Where(s => s.IsActive && s.IsVisible)
                    .OrderBy(s => s.Id)
                    .ToListAsync();
            }
        }
    }
}
