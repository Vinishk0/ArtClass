using System.Globalization;

namespace ArtClass.ViewModels;

internal static class DayOfWeekExtensions
{
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");

    public static string ToRussianName(this DayOfWeek day) =>
        Russian.DateTimeFormat.GetDayName(day);
}
