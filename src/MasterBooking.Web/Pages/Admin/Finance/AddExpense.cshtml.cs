using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Web.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Finance
{
    [Authorize]
    public class AddExpenseModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AddExpenseModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Укажите название расхода")]
            [StringLength(100, ErrorMessage = "Не длиннее 100 символов")]
            public string Title { get; set; } = string.Empty;

            [Range(0.01, 100000000, ErrorMessage = "Сумма должна быть больше 0")]
            public decimal Amount { get; set; }

            [Required(ErrorMessage = "Укажите дату")]
            public DateOnly? Date { get; set; }
        }

        public async Task OnGetAsync()
        {
            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var zone = MasterTime.Zone(settings?.TimeZoneId);
            Input.Date = DateOnly.FromDateTime(MasterTime.ToLocal(DateTime.UtcNow, zone));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var zone = MasterTime.Zone(settings?.TimeZoneId);

            _context.Expenses.Add(new Expense
            {
                Title = Input.Title.Trim(),
                Amount = Input.Amount,
                Date = MasterTime.ToUtc(Input.Date!.Value.ToDateTime(TimeOnly.MinValue), zone)
            });
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}
