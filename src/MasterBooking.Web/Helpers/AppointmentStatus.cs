namespace MasterBooking.Web.Helpers
{

    public static class AppointmentStatus
    {
        public static bool IsDone(string? status) => status is "Done" or "Completed";

        public static bool IsCancelled(string? status) => status == "Cancelled";

        public static string Text(string? status) =>
            IsDone(status) ? "Выполнена" : IsCancelled(status) ? "Отменена" : "Запланирована";

        public static string Css(string? status) =>
            IsDone(status) ? "g" : IsCancelled(status) ? "e" : "p";
    }
}
