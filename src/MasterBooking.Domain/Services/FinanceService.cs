using MasterBooking.Domain.Entities;
using MasterBooking.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterBooking.Domain.Services
{
    public class FinanceService : IFinanceService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public FinanceService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<FinanceStatistics> GetStatisticsAsync(int masterId, DateTime start, DateTime end)
        {
            var appointments = await _appointmentRepository.GetCompletedAppointmentsAsync(masterId, start, end);

            var statistics = new FinanceStatistics();

            if (!appointments.Any())
            {
                return statistics;
            }

            statistics.TotalIncome = appointments.Sum(a => a.FinalPrice);
            statistics.AverageCheck = statistics.TotalIncome / appointments.Count();

            statistics.IncomeByService = appointments
                .GroupBy(a => a.Service.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.FinalPrice)
                );

            return statistics;
        }
    }
}
