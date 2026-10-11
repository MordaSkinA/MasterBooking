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
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            [Required]
            public int Id { get; set; }
            [Required]
            public int ClientId { get; set; }
            [Required]
            public int ServiceId { get; set; }
            [Required]
            public DateTime StartDateTime { get; set; }
            [Required]
            public DateTime EndDateTime { get; set; }
            [Required]
            [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
            public decimal FinalPrice { get; set; }
            [Required]
            public string Status { get; set; } = "Planned";
        }

        public SelectList? Clients { get; set; }
        public SelectList? Services { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var timeZoneId = settings?.TimeZoneId ?? "Russian Standard Time";
            var masterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            Input = new InputModel
            {
                Id = appointment.Id,
                ClientId = appointment.ClientId ?? 0,
                ServiceId = appointment.ServiceId,
                StartDateTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.StartDateTime, masterTimeZone),
                EndDateTime = TimeZoneInfo.ConvertTimeFromUtc(appointment.EndDateTime, masterTimeZone),
                FinalPrice = appointment.FinalPrice,
                Status = appointment.Status
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

            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var timeZoneId = settings?.TimeZoneId ?? "Russian Standard Time";
            var masterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            var appointment = await _context.Appointments.FindAsync(Input.Id);
            if (appointment == null)
            {
                return NotFound();
            }

            appointment.ClientId = Input.ClientId;
            appointment.ServiceId = Input.ServiceId;
            appointment.StartDateTime = TimeZoneInfo.ConvertTimeToUtc(Input.StartDateTime, masterTimeZone);
            appointment.EndDateTime = TimeZoneInfo.ConvertTimeToUtc(Input.EndDateTime, masterTimeZone);
            appointment.FinalPrice = Input.FinalPrice;
            appointment.Status = Input.Status;

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

            _context.Update(appointment);
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
