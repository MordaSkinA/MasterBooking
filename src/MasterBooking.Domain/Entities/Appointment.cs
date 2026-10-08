using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        public int MasterId { get; set; }

        public Master Master { get; set; } = null!;

        public int? ClientId { get; set; }

        public Client? Client { get; set; }

        public int ServiceId { get; set; }

        public Service Service { get; set; } = null!;

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public string Status { get; set; } = "Planned";

        public decimal FinalPrice { get; set; }
    }
}