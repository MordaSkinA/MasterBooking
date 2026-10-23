using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages
{
    public class IndexModel : PublicPageModelBase
    {
        public IndexModel(ApplicationDbContext context) : base(context)
        {
        }

        public List<Service> Services { get; set; } = new();
        public List<GalleryImage> Gallery { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadPageDataAsync();
        }

        protected override async Task LoadPageDataAsync()
        {
            await LoadSettingsAsync();

            if (Settings == null)
            {
                return;
            }

            Services = await Context.Services.AsNoTracking()
                .Where(s => s.IsActive && s.IsVisible)
                .OrderBy(s => s.Id)
                .Take(3)
                .ToListAsync();
            Gallery = await Context.GalleryImages.AsNoTracking()
                .OrderBy(g => g.Order)
                .Take(4)
                .ToListAsync();
            Reviews = await Context.Reviews.AsNoTracking()
                .Where(r => r.Status == Review.Published)
                .OrderByDescending(r => r.CreatedAt)
                .Take(3)
                .ToListAsync();
        }
    }
}
