using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages
{

    public abstract class PublicPageModelBase : PageModel
    {
        protected readonly ApplicationDbContext Context;

        protected PublicPageModelBase(ApplicationDbContext context)
        {
            Context = context;
        }

        public SiteSettings? Settings { get; set; }

        [BindProperty]
        public RequestInput Input { get; set; } = new();

        [TempData]
        public bool RequestSent { get; set; }

        protected async Task LoadSettingsAsync()
        {
            Settings = await Context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();


            if (Settings is { IsPublished: false } && User.Identity?.IsAuthenticated != true)
            {
                Settings = null;
            }
        }


        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync();
                return Page();
            }

            Context.ServiceRequests.Add(new ServiceRequest
            {
                Name = Input.Name.Trim(),
                Phone = Input.Phone.Trim(),
                Comment = (Input.Comment ?? string.Empty).Trim(),
                Status = ServiceRequest.New,
                CreatedAt = DateTime.UtcNow
            });
            await Context.SaveChangesAsync();

            RequestSent = true;
            return RedirectToPage();
        }


        protected abstract Task LoadPageDataAsync();
    }
}
