using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class Expense
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public decimal Amount { get; set; }


        public DateTime Date { get; set; }
    }
}
