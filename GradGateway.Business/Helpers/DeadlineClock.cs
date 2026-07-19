namespace GradGateway.Business.Helpers;

/// <summary>
/// Application deadlines are evaluated by calendar date in Sri Lanka (en-LK).
/// Example: deadline 2 Jun 2026 → due from 00:00 on 3 Jun 2026 in Sri Lanka.
/// </summary>
public static class DeadlineClock
{
    private static readonly TimeZoneInfo SriLankaTimeZone = ResolveSriLankaTimeZone();

    public static DateTime TodayDateInSriLanka()
    {
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, SriLankaTimeZone);
        return localNow.Date;
    }

    public static bool IsDeadlinePassed(DateTime deadlineAtUtcOrUnspecified)
    {
        var deadlineDate = deadlineAtUtcOrUnspecified.Date;
        return deadlineDate < TodayDateInSriLanka();
    }

    private static TimeZoneInfo ResolveSriLankaTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Asia/Colombo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Sri Lanka Standard Time");
        }
    }
}
