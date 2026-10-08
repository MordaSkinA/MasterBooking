using MasterBooking.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MasterBooking.Domain.Interfaces
{
    public interface IFinanceService
    {
        Task<FinanceStatistics> GetStatisticsAsync(int masterId, DateTime start, DateTime end);
    }

    public class FinanceStatistics
    {
        public decimal TotalIncome { get; set; }
        public decimal AverageCheck { get; set; }
        public Dictionary<string, decimal> IncomeByService { get; set; } = new();
    }
}