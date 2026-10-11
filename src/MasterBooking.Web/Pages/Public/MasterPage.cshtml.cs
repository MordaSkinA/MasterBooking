using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages.Public
{
    public class MasterPageModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MasterPageModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSettings? Settings { get; set; }
        public List<Service> Services { get; set; } = new();
        public List<GalleryImage> Gallery { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Settings = await _context.SiteSettings.FirstOrDefaultAsync();

            if (Settings == null)
            {
                return NotFound();
            }

            Services = await _context.Services
                .Where(s => s.IsActive)
                .ToListAsync();
            Gallery = await _context.GalleryImages
                .OrderBy(g => g.Order)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetBusyIntervalsAsync()
        {
            var appointments = await _context.Appointments
                .Select(a => new
                {
                    start = a.StartDateTime,
                    end = a.EndDateTime,
                    display = "background"
                })
                .ToListAsync();

            return new JsonResult(appointments);
        }
    }
}
