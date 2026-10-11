using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class DayOff
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }
    }
}
