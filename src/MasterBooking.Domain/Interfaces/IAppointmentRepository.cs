using MasterBooking.Domain.Entities;

namespace MasterBooking.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAppointmentsAsync(int masterId, DateTime start, DateTime end);
        Task<IEnumerable<WorkingHours>> GetWorkingHoursAsync(int masterId);
        Task<IEnumerable<DayOff>> GetDayOffsAsync(int masterId);
    }
}
