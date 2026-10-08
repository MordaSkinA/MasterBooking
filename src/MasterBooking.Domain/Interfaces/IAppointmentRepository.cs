using MasterBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MasterBooking.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAppointmentsAsync(int masterId, DateTime start, DateTime end);
        Task<IEnumerable<WorkingHours>> GetWorkingHoursAsync(int masterId);
        Task<IEnumerable<DayOff>> GetDayOffsAsync(int masterId);
        Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync(int masterId, DateTime start, DateTime end);
    }
}
