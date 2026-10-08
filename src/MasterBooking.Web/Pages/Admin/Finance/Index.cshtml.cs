using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;

namespace MasterBooking.Web.Pages.Admin.Finance
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IFinanceService _financeService;
        private readonly IMasterProvider _masterProvider;

        public IndexModel(IFinanceService financeService, IMasterProvider masterProvider)
        {
            _financeService = financeService;
            _masterProvider = masterProvider;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
            public DateTime EndDate { get; set; } = DateTime.Now;
        }

        public FinanceStatistics? Statistics { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadStatistics();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await LoadStatistics();
            return Page();
        }

        private async Task LoadStatistics()
        {
            var masterId = _masterProvider.GetMasterId();
            if (masterId == null)
            {
                return;
            }

            Statistics = await _financeService.GetStatisticsAsync(masterId.Value, Input.StartDate, Input.EndDate);
        }
    }
}