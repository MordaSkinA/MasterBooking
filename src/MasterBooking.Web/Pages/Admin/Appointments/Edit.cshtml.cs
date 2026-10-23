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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;

        public EditModel(ApplicationDbContext context, IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Выберите клиента")]
            public int? ClientId { get; set; }

            [Required(ErrorMessage = "Выберите услугу")]
            public int? ServiceId { get; set; }

            [Required(ErrorMessage = "Укажите дату")]
            public DateOnly? Date { get; set; }

            [Required(ErrorMessage = "Укажите время")]
            public TimeOnly? Time { get; set; }

            [Range(0, double.MaxValue, ErrorMessage = "Цена не может быть отрицательной")]
            public decimal FinalPrice { get; set; }

 
            [Required]
            public string Status { get; set; } = "Planned";


            [Required]
            public string PaymentMethod { get; set; } = "Cash";

            [StringLength(500, ErrorMessage = "Не длиннее 500 символов")]
            public string? Notes { get; set; }
        }

        public SelectList? Clients { get; set; }
        public SelectList? Services { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var appointment = await _context.Appointments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var start = MasterTime.ToLocal(appointment.StartDateTime, MasterTime.Zone(settings?.TimeZoneId));

            Input = new InputModel
            {
                Id = appointment.Id,
                ClientId = appointment.ClientId,
                ServiceId = appointment.ServiceId,
                Date = DateOnly.FromDateTime(start),
                Time = TimeOnly.FromDateTime(start),
                FinalPrice = appointment.FinalPrice,
                Status = AppointmentStatus.IsDone(appointment.Status) ? "Done" : AppointmentStatus.IsCancelled(appointment.Status) ? "Cancelled" : "Planned",
                PaymentMethod = appointment.PaymentMethod,
                Notes = appointment.Notes
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

            var appointment = await _context.Appointments.FindAsync(Input.Id);
            var service = await _context.Services.FindAsync(Input.ServiceId);
            if (appointment == null)
            {
                return NotFound();
            }

            if (service == null || !await _context.Clients.AnyAsync(c => c.Id == Input.ClientId))
            {
                ModelState.AddModelError(string.Empty, "Клиент или услуга не найдены.");
                await PopulateSelectLists();
                return Page();
            }

            var settings = await _context.SiteSettings.AsNoTracking().FirstOrDefaultAsync();
            var zone = MasterTime.Zone(settings?.TimeZoneId);
            var startUtc = MasterTime.ToUtc(Input.Date!.Value.ToDateTime(Input.Time!.Value), zone);

            appointment.ClientId = Input.ClientId;
            appointment.ServiceId = service.Id;
            appointment.StartDateTime = startUtc;
            appointment.EndDateTime = startUtc.AddMinutes(service.Duration);
            appointment.FinalPrice = Input.FinalPrice;
            appointment.Status = Input.Status is "Done" or "Cancelled" ? Input.Status : "Planned";
            appointment.PaymentMethod = Input.PaymentMethod is "Card" or "Transfer" ? Input.PaymentMethod : "Cash";
            appointment.Notes = Input.Notes?.Trim() ?? string.Empty;

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

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Index");
        }

        private async Task PopulateSelectLists()
        {
            Clients = new SelectList(await _context.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            Services = new SelectList(await _context.Services.AsNoTracking().OrderBy(s => s.Id).ToListAsync(), "Id", "Name");
        }
    }
}
