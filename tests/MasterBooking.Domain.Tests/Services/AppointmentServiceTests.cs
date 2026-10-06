using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Exceptions;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Domain.Services;
using Moq;
using Xunit;

namespace MasterBooking.Domain.Tests.Services
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _repositoryMock;
        private readonly AppointmentService _service;
        private readonly int _masterId = 1;

        public AppointmentServiceTests()
        {
            _repositoryMock = new Mock<IAppointmentRepository>();
            _service = new AppointmentService(_repositoryMock.Object);
        }

        [Fact]
        public async Task ValidateAppointmentAsync_Throws_When_StartAfterEnd()
        {
            var appointment = new Appointment
            {
                StartDateTime = DateTime.Now.AddHours(1),
                EndDateTime = DateTime.Now
            };

            await Assert.ThrowsAsync<AppointmentValidationException>(() =>
                _service.ValidateAppointmentAsync(appointment, _masterId));
        }

        [Fact]
        public async Task ValidateAppointmentAsync_Throws_When_OnDayOff()
        {
            var appointment = new Appointment
            {
                StartDateTime = new DateTime(2023, 10, 10, 10, 0, 0),
                EndDateTime = new DateTime(2023, 10, 10, 11, 0, 0)
            };

            _repositoryMock.Setup(r => r.GetDayOffsAsync(_masterId))
                .ReturnsAsync(new List<DayOff> { new DayOff { Date = new DateTime(2023, 10, 10) } });

            await Assert.ThrowsAsync<AppointmentValidationException>(() =>
                _service.ValidateAppointmentAsync(appointment, _masterId));
        }

        [Fact]
        public async Task ValidateAppointmentAsync_Throws_When_OutsideWorkingHours()
        {
            var appointment = new Appointment
            {
                StartDateTime = new DateTime(2023, 10, 10, 22, 0, 0), // 10 PM
                EndDateTime = new DateTime(2023, 10, 10, 23, 0, 0)
            };

            _repositoryMock.Setup(r => r.GetDayOffsAsync(_masterId))
                .ReturnsAsync(new List<DayOff>());

            _repositoryMock.Setup(r => r.GetWorkingHoursAsync(_masterId))
                .ReturnsAsync(new List<WorkingHours>
                {
                    new WorkingHours { DayOfWeek = (int)DayOfWeek.Tuesday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(18, 0, 0) }
                });

            // 2023-10-10 is a Tuesday
            await Assert.ThrowsAsync<AppointmentValidationException>(() =>
                _service.ValidateAppointmentAsync(appointment, _masterId));
        }

        [Fact]
        public async Task ValidateAppointmentAsync_Throws_When_Overlapping()
        {
            var appointment = new Appointment
            {
                Id = 1,
                StartDateTime = new DateTime(2023, 10, 10, 10, 0, 0),
                EndDateTime = new DateTime(2023, 10, 10, 11, 0, 0)
            };

            var existing = new Appointment
            {
                Id = 2,
                StartDateTime = new DateTime(2023, 10, 10, 10, 30, 0),
                EndDateTime = new DateTime(2023, 10, 10, 11, 30, 0)
            };

            _repositoryMock.Setup(r => r.GetDayOffsAsync(_masterId)).ReturnsAsync(new List<DayOff>());
            _repositoryMock.Setup(r => r.GetWorkingHoursAsync(_masterId))
                .ReturnsAsync(new List<WorkingHours>
                {
                    new WorkingHours { DayOfWeek = (int)DayOfWeek.Tuesday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(20, 0, 0) }
                });
            _repositoryMock.Setup(r => r.GetAppointmentsAsync(_masterId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Appointment> { existing });

            await Assert.ThrowsAsync<AppointmentValidationException>(() =>
                _service.ValidateAppointmentAsync(appointment, _masterId));
        }

        [Fact]
        public async Task ValidateAppointmentAsync_Succeeds_When_Valid()
        {
            var appointment = new Appointment
            {
                Id = 1,
                StartDateTime = new DateTime(2023, 10, 10, 10, 0, 0),
                EndDateTime = new DateTime(2023, 10, 10, 11, 0, 0)
            };

            _repositoryMock.Setup(r => r.GetDayOffsAsync(_masterId)).ReturnsAsync(new List<DayOff>());
            _repositoryMock.Setup(r => r.GetWorkingHoursAsync(_masterId))
                .ReturnsAsync(new List<WorkingHours>
                {
                    new WorkingHours { DayOfWeek = (int)DayOfWeek.Tuesday, StartTime = new TimeSpan(8, 0, 0), EndTime = new TimeSpan(20, 0, 0) }
                });
            _repositoryMock.Setup(r => r.GetAppointmentsAsync(_masterId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Appointment>());

            await _service.ValidateAppointmentAsync(appointment, _masterId);
        }
    }
}
