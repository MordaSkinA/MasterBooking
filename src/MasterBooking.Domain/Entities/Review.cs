using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class Review
    {
        public const string Pending = "Pending";
        public const string Published = "Published";
        public const string Hidden = "Hidden";

        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ClientName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Text { get; set; } = string.Empty;


        [Required]
        [StringLength(20)]
        public string Status { get; set; } = Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
