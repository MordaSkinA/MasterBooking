using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages
{
    public class WorksModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public WorksModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSettings? Settings { get; set; }
        public List<GalleryImage> Gallery { get; set; } = new();

        public async Task OnGetAsync()
        {
            Settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (Settings != null)
            {
                Gallery = await _context.GalleryImages
                    .OrderBy(g => g.Order)
                    .ToListAsync();
            }
        }
    }
}
