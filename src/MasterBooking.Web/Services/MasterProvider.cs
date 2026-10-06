using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using System.Security.Claims;

namespace MasterBooking.Web.Services
{
    public class MasterProvider : IMasterProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;

        public MasterProvider(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        public int? GetMasterId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity!.IsAuthenticated)
            {
                return null;
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }

            // Use a scope to resolve ApplicationDbContext
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var master = dbContext.Masters.FirstOrDefault(m => m.UserId == userId);
            return master?.Id;
        }
    }
}