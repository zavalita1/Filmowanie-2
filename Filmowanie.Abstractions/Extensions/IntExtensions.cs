using Filmowanie.Abstractions.Enums;

namespace Filmowanie.Abstractions.Extensions;

public static class IntExtensions
{
    public static Decade ToDecade(this int year)
    {
        var yearWithLeadingZero = year - year % 10;
        var result = yearWithLeadingZero switch
        {
            1940 => Decade._1940s,
            1950 => Decade._1950s,
            1960 => Decade._1960s,
            1970 => Decade._1970s,
            1980 => Decade._1980s,
            1990 => Decade._1990s,
            2000 => Decade._2000s,
            2010 => Decade._2010s,
            2020 => Decade._2020s,
            _ => throw new ArgumentOutOfRangeException($"Can't parse provided {nameof(year)}: '{year}'!")
        };

        return result;
    }

    public static string GetDurationString(this int durationInMinutes)
    {
        if (durationInMinutes <= 0)
            throw new ArgumentException($"Duration must be positive! Provided: {durationInMinutes}!");

        var duration = TimeSpan.FromMinutes(durationInMinutes);
        var durationHours = (int)duration.TotalHours;
        var hoursPart = durationHours == 0 ? string.Empty : $"{durationHours} godz.";
        var minutes = duration.Minutes;
        var minutesPart = minutes == 0 ? string.Empty : $"{minutes} min.";
        var result = $"{hoursPart} {minutesPart}";
        return result.Trim();
    }
}