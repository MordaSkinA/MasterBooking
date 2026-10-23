using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Services
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public int Id { get; set; }

            [Required(ErrorMessage = "Укажите название")]
            [StringLength(100, ErrorMessage = "Не длиннее 100 символов")]
            public string Name { get; set; } = string.Empty;

            [Range(0, double.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
            public decimal Price { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Длительность должна быть больше 0")]
            public int Duration { get; set; }


            public bool IsVisible { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            Input = new InputModel
            {
                Id = service.Id,
                Name = service.Name,
                Price = service.Price,
                Duration = service.Duration,
                IsVisible = service.IsVisible
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var service = await _context.Services.FindAsync(Input.Id);
            if (service == null)
            {
                return NotFound();
            }

            service.Name = Input.Name;
            service.Price = Input.Price;
            service.Duration = Input.Duration;
            service.IsVisible = Input.IsVisible;

            _context.Update(service);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}