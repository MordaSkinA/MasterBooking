using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Infrastructure.Data;
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
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            [Required]
            public int ClientId { get; set; }
            [Required]
            public int ServiceId { get; set; }
            [Required]
            public DateTime StartDateTime { get; set; } = DateTime.UtcNow.AddHours(1);
            [Required]
            [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
            public decimal FinalPrice { get; set; }
            [Required]
            public string Status { get; set; } = "Planned";
        }

        public SelectList? Clients { get; set; }
        public SelectList? Services { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
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

            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var timeZoneId = settings?.TimeZoneId ?? "Russian Standard Time";
            var masterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Convert input local time to UTC
            var startDateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(Input.StartDateTime, masterTimeZone);

            var appointment = new Appointment
            {
                ClientId = Input.ClientId,
                ServiceId = Input.ServiceId,
                StartDateTime = startDateTimeUtc,
                EndDateTime = startDateTimeUtc, // Will be updated below
                Status = Input.Status,
                FinalPrice = Input.FinalPrice
            };

            var service = await _context.Services.FindAsync(Input.ServiceId);
            if (service != null)
            {
                appointment.EndDateTime = appointment.StartDateTime.AddMinutes(service.Duration);
            }

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

            return RedirectToPage("./Index");
        }

        private async Task PopulateSelectLists()
        {
            Clients = new SelectList(await _context.Clients.ToListAsync(), "Id", "Name");
            Services = new SelectList(await _context.Services.ToListAsync(), "Id", "Name");
        }
    }
}
