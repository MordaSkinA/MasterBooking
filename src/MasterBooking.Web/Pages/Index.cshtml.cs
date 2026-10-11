using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSettings? Settings { get; set; }
        public List<Service> Services { get; set; } = new();
        public List<GalleryImage> Gallery { get; set; } = new();

        public async Task OnGetAsync()
        {
            Settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (Settings == null)
            {
                return;
            }

            Services = await _context.Services
                .Where(s => s.IsActive)
                .ToListAsync();
            Gallery = await _context.GalleryImages
                .OrderBy(g => g.Order)
                .ToListAsync();
        }
    }
}
