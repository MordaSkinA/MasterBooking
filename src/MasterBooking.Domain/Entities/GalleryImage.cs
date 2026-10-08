using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class GalleryImage
    {
        public int Id { get; set; }

        public int MasterId { get; set; }

        public Master Master { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string ImagePath { get; set; } = string.Empty;

        public int Order { get; set; }
    }
}