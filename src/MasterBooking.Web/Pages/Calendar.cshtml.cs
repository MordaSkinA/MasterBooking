using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MasterBooking.Domain.Entities;
using MasterBooking.Infrastructure.Data;
using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages
{
    public class CalendarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CalendarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SiteSettings? Settings { get; set; }
        public List<Appointment> BusyAppointments { get; set; } = new();

        [BindProperty]
        public BookingInput Input { get; set; } = new();

        public class BookingInput
        {
            [Required]
            public string Name { get; set; } = string.Empty;
            [Required]
            public string Phone { get; set; } = string.Empty;
            [Required]
            public DateTime DateTime { get; set; }
        }

        public async Task OnGetAsync()
        {
            Settings = await _context.SiteSettings.FirstOrDefaultAsync();

            // Get appointments for the next 30 days
            BusyAppointments = await _context.Appointments
                .Where(a => a.StartDateTime >= DateTime.UtcNow.AddDays(-7) && a.StartDateTime <= DateTime.UtcNow.AddDays(30))
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var settings = await _context.SiteSettings.FirstOrDefaultAsync();
            var timeZoneId = settings?.TimeZoneId ?? "Russian Standard Time";
            var masterTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            // Input.DateTime is parsed as Local or Unspecified. Convert it to UTC.
            var dateTimeUtc = TimeZoneInfo.ConvertTimeToUtc(Input.DateTime, masterTimeZone);

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == Input.Phone);
            if (client == null)
            {
                client = new Client
                {
                    Name = Input.Name,
                    PhoneNumber = Input.Phone,
                    MasterId = settings?.UserId ?? string.Empty
                };
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();
            }

            var service = await _context.Services.FirstOrDefaultAsync(); // Default service

            if (service == null)
            {
                ModelState.AddModelError(string.Empty, "Услуги не найдены.");
                return Page();
            }

            var appointment = new Appointment
            {
                ClientId = client.Id,
                ServiceId = service.Id,
                StartDateTime = dateTimeUtc,
                EndDateTime = dateTimeUtc.AddMinutes(service.Duration),
                Status = "New",
                FinalPrice = service.Price
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }
    }
}
