using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Showcase
{
    [Authorize]
    public class AddReviewModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddReviewModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Укажите имя клиента")]
            [StringLength(100, ErrorMessage = "Не длиннее 100 символов")]
            public string ClientName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Введите текст отзыва")]
            [StringLength(2000, ErrorMessage = "Не длиннее 2000 символов")]
            public string Text { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


            _context.Reviews.Add(new Review
            {
                ClientName = Input.ClientName.Trim(),
                Text = Input.Text.Trim(),
                Status = Review.Published,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
