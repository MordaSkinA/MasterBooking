using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class Appointment
    {
        public int Id { get; set; }

        public int? ClientId { get; set; }

        public Client? Client { get; set; }

        public int ServiceId { get; set; }

        public Service Service { get; set; } = null!;

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public string Status { get; set; } = "Planned";

        public decimal FinalPrice { get; set; }


        [StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash";

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;
    }
}
