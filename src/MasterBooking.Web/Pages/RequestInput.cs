using System.ComponentModel.DataAnnotations;

namespace MasterBooking.Web.Pages
{

    public class RequestInput
    {
        [Required(ErrorMessage = "Укажите имя")]
        [StringLength(100, ErrorMessage = "Не длиннее 100 символов")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите телефон")]
        [StringLength(50, ErrorMessage = "Не длиннее 50 символов")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Не длиннее 1000 символов")]
        public string? Comment { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Нужно согласие на обработку персональных данных")]
        public bool Consent { get; set; }
    }
}
