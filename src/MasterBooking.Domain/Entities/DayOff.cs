using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class DayOff
    {
        public int Id { get; set; }

        public int MasterId { get; set; }

        public Master Master { get; set; }

        public DateTime Date { get; set; }
    }
}