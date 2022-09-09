using System;

namespace RestfulFirebase.Utilities;

/// <summary>
/// Provides <see cref="TimeSpan"/> extensions.
/// </summary>
public static class TimeSpanExtensions
{
    private const int Second = 1;
    private const int Minute = 60 * Second;
    private const int Hour = 60 * Minute;
    private const int Day = 24 * Hour;
    //private const int Week = 7 * Day;
    private const int Month = 30 * Day;
    //private const int Year = 12 * Month;
    
    /// <summary>
    /// Gets nicely formatted time span such as "just now", "a minute" etc.
    /// </summary>
    /// <param name="timeSpan">
    /// </param>
    /// <returns>
    /// The nicely formatted time span representation of the provided <paramref name="timeSpan"/> parameter.
    /// </returns>
    public static string GetNiceFormattedTimeSpan(this TimeSpan timeSpan)
    {
        double delta = Math.Abs(timeSpan.TotalSeconds);

        if (delta < 1 * Minute)
            return "just now";

        if (delta < 2 * Minute)
            return "a minute";

        if (delta < 60 * Minute)
            return timeSpan.Minutes + " minutes";

        if (delta < 2 * Hour)
            return "an hour";

        if (delta < 24 * Hour)
            return timeSpan.Hours + " hours";

        if (delta < 48 * Hour)
            return "yesterday";

        if (delta < 30 * Day)
            return timeSpan.Days + " days";

        if (delta < 12 * Month)
        {
            int months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
            return months <= 1 ? "a month" : months + " months";
        }

        int years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
        return years <= 1 ? "a year" : years + " years";
    }
}
