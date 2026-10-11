using MasterBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MasterBooking.Domain.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAppointmentsAsync(DateTime start, DateTime end);
        Task<IEnumerable<WorkingHours>> GetWorkingHoursAsync();
        Task<IEnumerable<DayOff>> GetDayOffsAsync();
        Task<IEnumerable<Appointment>> GetCompletedAppointmentsAsync(DateTime start, DateTime end);
    }
}
