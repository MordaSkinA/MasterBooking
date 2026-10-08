using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Services
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IMasterProvider _masterProvider;

        public CreateModel(ApplicationDbContext context, IMasterProvider masterProvider)
        {
            _context = context;
            _masterProvider = masterProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
            public decimal Price { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Длительность должна быть больше 0")]
            public int Duration { get; set; }

            public bool IsActive { get; set; } = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var masterId = _masterProvider.GetMasterId();
            if (masterId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var service = new Service
            {
                Name = Input.Name,
                Price = Input.Price,
                Duration = Input.Duration,
                IsActive = Input.IsActive,
                MasterId = masterId.Value
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}