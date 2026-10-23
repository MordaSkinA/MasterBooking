namespace MasterBooking.Web.Helpers
{

    public static class MasterTime
    {
        public static TimeZoneInfo Zone(string? id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            try { return TimeZoneInfo.FindSystemTimeZoneById("Asia/Vladivostok"); }
            catch (Exception) { return TimeZoneInfo.Utc; }
        }

        public static DateTime ToLocal(DateTime utc, TimeZoneInfo zone) =>
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone);

        public static DateTime ToUtc(DateTime local, TimeZoneInfo zone) =>
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), zone);
    }
}
