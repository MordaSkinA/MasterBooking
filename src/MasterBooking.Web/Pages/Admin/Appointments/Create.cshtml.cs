using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Infrastructure.Data;
using MasterBooking.Web.Helpers;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages.Admin.Appointments
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;

        public CreateModel(ApplicationDbContext context, IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Выберите клиента")]
            public int? ClientId { get; set; }

            [Required(ErrorMessage = "Выберите услугу")]
            public int? ServiceId { get; set; }

            [Required(ErrorMessage = "Укажите дату")]
            public DateOnly? Date { get; set; }

            [Required(ErrorMessage = "Укажите время")]
            public TimeOnly? Time { get; set; }

            [StringLength(500, ErrorMessage = "Не длиннее 500 символов")]
            public string? Notes { get; set; }
        }

        public SelectList? Clients { get; set; }
        public SelectList? Services { get; set; }


        public async Task<IActionResult> OnGetAsync(int? clientId)
        {
            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var now = MasterTime.ToLocal(DateTime.UtcNow, MasterTime.Zone(settings?.TimeZoneId));
            var nextHour = now.Date.AddHours(now.Hour + 1);

            Input = new InputModel
            {
                ClientId = clientId,
                Date = DateOnly.FromDateTime(nextHour),
                Time = TimeOnly.FromDateTime(nextHour)
            };

            await PopulateSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateSelectLists();
                return Page();
            }

            var service = await _context.Services.FindAsync(Input.ServiceId);
            if (service == null || !await _context.Clients.AnyAsync(c => c.Id == Input.ClientId))
            {
                ModelState.AddModelError(string.Empty, "Клиент или услуга не найдены.");
                await PopulateSelectLists();
                return Page();
            }

            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var zone = MasterTime.Zone(settings?.TimeZoneId);
            var startUtc = MasterTime.ToUtc(Input.Date!.Value.ToDateTime(Input.Time!.Value), zone);

            var appointment = new Appointment
            {
                ClientId = Input.ClientId,
                ServiceId = service.Id,
                StartDateTime = startUtc,
                EndDateTime = startUtc.AddMinutes(service.Duration),
                Status = "Planned",
                FinalPrice = service.Price,
                PaymentMethod = "Cash",
                Notes = Input.Notes?.Trim() ?? string.Empty
            };

            try
            {
                await _appointmentService.ValidateAppointmentAsync(appointment);
            }
            catch (AppointmentValidationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await PopulateSelectLists();
                return Page();
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Index");
        }

        private async Task PopulateSelectLists()
        {
            Clients = new SelectList(await _context.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            Services = new SelectList(await _context.Services.AsNoTracking().Where(s => s.IsActive).OrderBy(s => s.Id).ToListAsync(), "Id", "Name");
        }
    }
}
