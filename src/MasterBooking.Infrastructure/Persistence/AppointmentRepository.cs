using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MasterBooking.Infrastructure.Persistence
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(DateTime start, DateTime end)
        {
            return await _context.Appointments
                .Where(a => a.StartDateTime < end && a.EndDateTime > start)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkingHours>> GetWorkingHoursAsync()
        {
            return await _context.WorkingHours
                .ToListAsync();
        }

        public async Task<IEnumerable<DayOff>> GetDayOffsAsync()
        {
            return await _context.DayOffs
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync(DateTime start, DateTime end)
        {
            return await _context.Appointments
                .Include(a => a.Service)
                .Where(a => (a.Status == "Completed" || a.Status == "Done") &&
                            a.StartDateTime >= start &&
                            a.StartDateTime <= end)
                .ToListAsync();
        }
    }
}
