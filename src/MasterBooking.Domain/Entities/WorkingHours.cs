using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class WorkingHours
    {
        public int Id { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}
