namespace Filmowanie.Abstractions.Helpers;

public static class StringHelper
{
    public static string GetDurationString(int durationInMinutes)
    {
        var duration = TimeSpan.FromMinutes(durationInMinutes);
        var durationHours = (int)duration.TotalHours;
        var durationMinutes = (int)duration.TotalMinutes - 60 * durationHours;
        var result = durationMinutes == 0 ? $"{durationHours} godz." : $"{durationHours} godz. {durationMinutes} min.";
        return result;
    }
}