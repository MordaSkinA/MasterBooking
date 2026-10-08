using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Clients
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

            [StringLength(50)]
            public string? PhoneNumber { get; set; }
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

            var client = new Client
            {
                Name = Input.Name,
                PhoneNumber = Input.PhoneNumber,
                MasterId = masterId.Value
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}