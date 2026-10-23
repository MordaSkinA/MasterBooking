using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MasterBooking.Web.Pages.Admin.Appointments
{

    [Authorize]
    public class IndexModel : PageModel
    {
        public IActionResult OnGet() => RedirectToPage("/Admin/Index");
    }
}
