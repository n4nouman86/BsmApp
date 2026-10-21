namespace BlazorPortal.MethodExtensions;

public static class DateTimeExtension
{

    public static DateTimeOffset ToLacalDateTimeOffset(this DateTimeOffset dateTimeOffset, string clientTimeZoneInfoId)
    {
        var dt = DateTime.UtcNow;

        var tz = TimeZoneInfo.FindSystemTimeZoneById(clientTimeZoneInfoId);
        var utcOffset = new DateTimeOffset(dt, TimeSpan.Zero);

        return dateTimeOffset.ToOffset(tz.GetUtcOffset(utcOffset));
    }






    public static DateTimeOffset ToServerDateTimeOffset(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToLocalTime();
    }






    public static string ToDateTimeString(this DateTime dateTime) 
    { 
        return dateTime.ToString("dd MMM yyyy, hh':'mm tt"); 
    }





    public static string ToDateString(this DateTime dateTime)
    {
        return dateTime.ToString("dd MMM yyyy");
    }
}
