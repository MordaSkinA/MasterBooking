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

        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(int masterId, DateTime start, DateTime end)
        {
            return await _context.Appointments
                .Where(a => a.MasterId == masterId && a.StartDateTime < end && a.EndDateTime > start)
                .ToListAsync();
        }

        public async Task<IEnumerable<WorkingHours>> GetWorkingHoursAsync(int masterId)
        {
            return await _context.WorkingHours
                .Where(w => w.MasterId == masterId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DayOff>> GetDayOffsAsync(int masterId)
        {
            return await _context.DayOffs
                .Where(d => d.MasterId == masterId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync(int masterId, DateTime start, DateTime end)
        {
            return await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.MasterId == masterId &&
                            a.Status == "Done" &&
                            a.StartDateTime >= start &&
                            a.StartDateTime <= end)
                .ToListAsync();
        }
    }
}
