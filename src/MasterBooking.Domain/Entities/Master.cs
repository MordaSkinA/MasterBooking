using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class Master
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;
    }
}