using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class Service
    {
        public int Id { get; set; }

        public int MasterId { get; set; }

        public Master Master { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; } 

        public bool IsActive { get; set; } = true;
    }
}