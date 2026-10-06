using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Infrastructure.Data;

namespace MasterBooking.Web.Pages.Admin.Appointments
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppointmentService _appointmentService;
        private readonly IMasterProvider _masterProvider;

        public CreateModel(ApplicationDbContext context, IAppointmentService appointmentService, IMasterProvider masterProvider)
        {
            _context = context;
            _appointmentService = appointmentService;
            _masterProvider = masterProvider;
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
            public DateTime StartDateTime { get; set; } = DateTime.Now.AddHours(1);
            [Required]
            [Range(0, double.MaxValue, ErrorMessage = "Цена должна быть положительной")]
            public decimal FinalPrice { get; set; }
            [Required]
            public string Status { get; set; } = "Planned";
        }

        public SelectList Clients { get; set; } = new();
        public SelectList Services { get; set; } = new();

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

            var masterId = _masterProvider.GetMasterId();
            if (masterId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var appointment = new Appointment
            {
                MasterId = masterId.Value,
                ClientId = Input.ClientId,
                ServiceId = Input.ServiceId,
                StartDateTime = Input.StartDateTime,
                EndDateTime = Input.StartDateTime.AddMinutes(30), // Placeholder, should ideally be based on service duration
                Status = Input.Status,
                FinalPrice = Input.FinalPrice
            };

            // We should ideally use the service's duration if available.
            var service = await _context.Services.FindAsync(Input.ServiceId);
            if (service != null)
            {
                appointment.EndDateTime = appointment.StartDateTime.AddMinutes(service.Duration);
            }

            try
            {
                await _appointmentService.ValidateAppointmentAsync(appointment, masterId.Value);
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
