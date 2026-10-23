using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Clients
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
            [Required]
            [StringLength(100)]
            public string Name { get; set; } = string.Empty;

            [StringLength(50)]
            public string? PhoneNumber { get; set; }

            public string? Notes { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var client = new Client
            {
                Name = Input.Name,
                PhoneNumber = Input.PhoneNumber ?? string.Empty,
                Notes = Input.Notes ?? string.Empty
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
