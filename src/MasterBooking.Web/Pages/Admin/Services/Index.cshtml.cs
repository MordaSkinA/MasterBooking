using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages.Admin.Services
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Service> Services { get; set; } = new List<Service>();

        public async Task OnGetAsync()
        {
            Services = await _context.Services.AsNoTracking().OrderBy(s => s.Id).ToListAsync();
        }


        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                service.IsVisible = !service.IsVisible;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
