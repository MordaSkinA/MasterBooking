using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Services
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Укажите название")]
            [StringLength(100, ErrorMessage = "Не длиннее 100 символов")]
            public string Name { get; set; } = string.Empty;

            [Range(0, double.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
            public decimal Price { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Длительность должна быть больше 0")]
            public int Duration { get; set; }

    
            public bool IsVisible { get; set; } = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var service = new Service
            {
                Name = Input.Name,
                Price = Input.Price,
                Duration = Input.Duration,
                IsActive = true,
                IsVisible = Input.IsVisible
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
