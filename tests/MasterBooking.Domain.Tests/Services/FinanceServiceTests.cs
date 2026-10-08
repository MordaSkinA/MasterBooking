using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using MasterBooking.Domain.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MasterBooking.Domain.Tests.Services
{
    public class FinanceServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
        private readonly FinanceService _financeService;

        public FinanceServiceTests()
        {
            _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            _financeService = new FinanceService(_appointmentRepositoryMock.Object);
        }

        [Fact]
        public async Task GetStatisticsAsync_ReturnsEmptyStatistics_WhenNoAppointmentsFound()
        {
            int masterId = 1;
            var start = DateTime.Now.AddDays(-7);
            var end = DateTime.Now;

            _appointmentRepositoryMock
                .Setup(r => r.GetCompletedAppointmentsAsync(masterId, start, end))
                .ReturnsAsync(Enumerable.Empty<Appointment>());

            var result = await _financeService.GetStatisticsAsync(masterId, start, end);

            Assert.Equal(0, result.TotalIncome);
            Assert.Equal(0, result.AverageCheck);
            Assert.Empty(result.IncomeByService);
        }

        [Fact]
        public async Task GetStatisticsAsync_CalculatesCorrectTotals_WhenAppointmentsExist()
        {
            int masterId = 1;
            var start = DateTime.Now.AddDays(-7);
            var end = DateTime.Now;

            var appointments = new List<Appointment>
            {
                new Appointment { Id = 1, FinalPrice = 1000m, Service = new Service { Name = "Haircut" } },
                new Appointment { Id = 2, FinalPrice = 2000m, Service = new Service { Name = "Coloring" } },
                new Appointment { Id = 3, FinalPrice = 1500m, Service = new Service { Name = "Haircut" } }
            };

            _appointmentRepositoryMock
                .Setup(r => r.GetCompletedAppointmentsAsync(masterId, start, end))
                .ReturnsAsync(appointments);

            var result = await _financeService.GetStatisticsAsync(masterId, start, end);

            Assert.Equal(4500m, result.TotalIncome);
            Assert.Equal(4500m / 3, result.AverageCheck);
            Assert.Equal(2, result.IncomeByService.Count);
            Assert.Equal(2500m, result.IncomeByService["Haircut"]);
            Assert.Equal(2000m, result.IncomeByService["Coloring"]);
        }

        [Fact]
        public async Task GetStatisticsAsync_HandlesZeroDivision_WhenAppointmentsAreEmpty()
        {
            int masterId = 1;
            var start = DateTime.Now.AddDays(-7);
            var end = DateTime.Now;

            _appointmentRepositoryMock
                .Setup(r => r.GetCompletedAppointmentsAsync(masterId, start, end))
                .ReturnsAsync(new List<Appointment>());

            var result = await _financeService.GetStatisticsAsync(masterId, start, end);

            Assert.Equal(0, result.AverageCheck);
        }
    }
}
