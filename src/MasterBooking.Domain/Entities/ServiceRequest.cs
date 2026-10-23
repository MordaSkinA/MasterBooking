using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class ServiceRequest
    {
        public const string New = "New";
        public const string Contacted = "Contacted";
        public const string Converted = "Converted";
        public const string Rejected = "Rejected";

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public int? ServiceId { get; set; }

        public Service? Service { get; set; }


        [Required]
        [StringLength(20)]
        public string Status { get; set; } = New;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
