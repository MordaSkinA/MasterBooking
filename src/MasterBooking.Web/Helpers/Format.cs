using System.Globalization;

namespace MasterBooking.Web.Helpers
{
    public static class Format
    {
        private static readonly string[] Months =
        {
            "января", "февраля", "марта", "апреля", "мая", "июня",
            "июля", "августа", "сентября", "октября", "ноября", "декабря"
        };

        private static readonly string[] Weekdays =
        {
            "воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота"
        };

        public static string Rub(this decimal value)
        {
            var pattern = value % 1 == 0 ? "#,##0" : "#,##0.00";
            var text = value.ToString(pattern, CultureInfo.InvariantCulture).Replace(",", "\u00A0");
            return text + "\u00A0₽";
        }

        public static string Minutes(this int minutes)
        {
            if (minutes < 60)
            {
                return minutes + " мин";
            }

            var hours = minutes / 60;
            var rest = minutes % 60;
            return rest == 0 ? hours + " ч" : hours + " ч " + rest + " мин";
        }

        public static string LongDate(this DateTime date)
        {
            return Weekdays[(int)date.DayOfWeek] + ", " + date.Day + " " + Months[date.Month - 1];
        }

        public static string DayMonth(this DateTime date)
        {
            return date.Day + " " + Months[date.Month - 1];
        }

        public static string SafeColor(string? value, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(value) && System.Text.RegularExpressions.Regex.IsMatch(value.Trim(), "^#[0-9a-fA-F]{6}$"))
            {
                return value.Trim();
            }

            return fallback;
        }

        public static string ContrastText(string hex)
        {
            var r = Convert.ToInt32(hex.Substring(1, 2), 16);
            var g = Convert.ToInt32(hex.Substring(3, 2), 16);
            var b = Convert.ToInt32(hex.Substring(5, 2), 16);
            var luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
            return luminance > 0.62 ? "#1D2A27" : "#FFFFFF";
        }
    }
}
