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

        public Master Master { get; set; } = default!;
        public SiteSettings? Settings { get; set; }
        public List<Service> Services { get; set; } = new();
        public List<GalleryImage> Gallery { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string slug)
        {
            var master = await _context.Masters
                .Include(m => m.SiteSettings)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (master == null)
            {
                return NotFound();
            }

            Master = master;
            Settings = master.SiteSettings;
            Services = await _context.Services
                .Where(s => s.MasterId == master.Id && s.IsActive)
                .ToListAsync();
            Gallery = await _context.GalleryImages
                .Where(g => g.MasterId == master.Id)
                .OrderBy(g => g.Order)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnGetBusyIntervalsAsync(string slug)
        {
            var master = await _context.Masters
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (master == null)
            {
                return NotFound();
            }

            var appointments = await _context.Appointments
                .Where(a => a.MasterId == master.Id)
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
