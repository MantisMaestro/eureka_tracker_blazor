namespace Client.Extensions;

public static class DateTimeExtensions
{
    public static DateOnly StartOfWeek(this DateOnly date, DayOfWeek startOfWeek)
    {
        var diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-1 * diff);
    }
}