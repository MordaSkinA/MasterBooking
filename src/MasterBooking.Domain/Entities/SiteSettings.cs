using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Domain.Entities
{
    public class SiteSettings
    {
        public int Id { get; set; }

        [StringLength(200)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(50)]
        public string Theme { get; set; } = string.Empty;

        [StringLength(20)]
        public string PrimaryColor { get; set; } = string.Empty;

        [StringLength(200)]
        public string CoverImage { get; set; } = string.Empty;

        [StringLength(200)]
        public string Logo { get; set; } = string.Empty;

        public string AboutMe { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string SocialLinks { get; set; } = string.Empty;

        public bool ShowCalendar { get; set; } = true;


        public bool IsPublished { get; set; } = true;

        [StringLength(100)]
        public string TimeZoneId { get; set; } = "Russian Standard Time";
    }
}
