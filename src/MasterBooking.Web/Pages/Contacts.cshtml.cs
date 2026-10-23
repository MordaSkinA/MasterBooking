using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages
{
    public class ContactsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ContactsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSettings? Settings { get; set; }

        public async Task OnGetAsync()
        {
            Settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();

            if (Settings is { IsPublished: false } && User.Identity?.IsAuthenticated != true)
            {
                Settings = null;
            }
        }
    }
}
