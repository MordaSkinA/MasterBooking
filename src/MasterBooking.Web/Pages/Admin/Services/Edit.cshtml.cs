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
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [StringLength(100)]
            public int Id { get; set; }

            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
            public decimal Price { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Длительность должна быть больше 0")]
            public int Duration { get; set; }

            public bool IsActive { get; set; }
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
                IsActive = service.IsActive
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
            service.IsActive = Input.IsActive;

            _context.Update(service);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}