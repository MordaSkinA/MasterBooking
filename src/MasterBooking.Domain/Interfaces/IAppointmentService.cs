using MasterBooking.Domain.Entities;

namespace MasterBooking.Domain.Interfaces
{
    public interface IAppointmentService
    {
        Task ValidateAppointmentAsync(Appointment appointment, int masterId);
    }
}
