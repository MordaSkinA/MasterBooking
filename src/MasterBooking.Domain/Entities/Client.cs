using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class Client
    {
        public int Id { get; set; }

        public int MasterId { get; set; }

        public Master Master { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }
}