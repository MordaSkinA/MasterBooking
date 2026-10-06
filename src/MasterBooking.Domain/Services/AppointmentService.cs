using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Domain.Interfaces;

namespace MasterBooking.Domain.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _repository;

        public AppointmentService(IAppointmentRepository repository)
        {
            _repository = repository;
        }

        public async Task ValidateAppointmentAsync(Appointment appointment, int masterId)
        {
            if (appointment.StartDateTime >= appointment.EndDateTime)
            {
                throw new AppointmentValidationException("Конец записи должен быть позже начала.");
            }

            await ValidateDayOffsAsync(appointment, masterId);
            await ValidateWorkingHoursAsync(appointment, masterId);
            await ValidateOverlapsAsync(appointment, masterId);
        }

        private async Task ValidateDayOffsAsync(Appointment appointment, int masterId)
        {
            var dayOffs = await _repository.GetDayOffsAsync(masterId);

            for (var date = appointment.StartDateTime.Date; date <= appointment.EndDateTime.Date; date = date.AddDays(1))
            {
                if (dayOffs.Any(d => d.Date.Date == date))
                {
                    throw new AppointmentValidationException("В этот день у мастера выходной.");
                }
            }
        }

        private async Task ValidateWorkingHoursAsync(Appointment appointment, int masterId)
        {
            var workingHours = await _repository.GetWorkingHoursAsync(masterId);

            // For now, we only support appointments within a single day.
            // This is a reasonable assumption for a beauty salon.
            if (appointment.StartDateTime.Date != appointment.EndDateTime.Date)
            {
                throw new AppointmentValidationException("Запись не может длиться более одного дня.");
            }

            var dayOfWeek = (int)appointment.StartDateTime.DayOfWeek;
            var hoursForDay = workingHours.Where(w => (int)w.DayOfWeek == dayOfWeek).ToList();

            if (!hoursForDay.Any())
            {
                throw new AppointmentValidationException($"В этот день ({appointment.StartDateTime:dd.MM.yyyy}) мастер не работает.");
            }

            var start = appointment.StartDateTime.TimeOfDay;
            var end = appointment.EndDateTime.TimeOfDay;

            if (!hoursForDay.Any(h => h.StartTime <= start && h.EndTime >= end))
            {
                throw new AppointmentValidationException("Время записи не входит в рабочие часы.");
            }
        }

        private async Task ValidateOverlapsAsync(Appointment appointment, int masterId)
        {
            var existingAppointments = await _repository.GetAppointmentsAsync(
                masterId,
                appointment.StartDateTime,
                appointment.EndDateTime);

            foreach (var existing in existingAppointments)
            {
                if (existing.Id == appointment.Id) continue;

                if (appointment.StartDateTime < existing.EndDateTime && existing.StartDateTime < appointment.EndDateTime)
                {
                    throw new AppointmentValidationException("Это время уже занято другой записью.");
                }
            }
        }
    }
}
