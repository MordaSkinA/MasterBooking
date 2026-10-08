using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class SiteSettings
    {
        public int Id { get; set; }

        public int MasterId { get; set; }

        public Master Master { get; set; } = null!;

        [StringLength(50)]
        public string Theme { get; set; } = string.Empty;

        [StringLength(20)]
        public string PrimaryColor { get; set; } = string.Empty;

        [StringLength(200)]
        public string CoverImage { get; set; } = string.Empty;

        [StringLength(200)]
        public string Logo { get; set; } = string.Empty;

        public string AboutMe { get; set; } = string.Empty;

        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string SocialLinks { get; set; } = string.Empty;

        public bool ShowCalendar { get; set; } = true;
    }
}
