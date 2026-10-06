using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class WorkingHours
    {
        public int Id { get; set; }

        public int MasterId { get; set; }

        public Master Master { get; set; }

        public int DayOfWeek { get; set; } 

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}