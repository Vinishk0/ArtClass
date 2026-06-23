using System.Globalization;

namespace ArtClass.ViewModels;

internal static class DateFormatting
{
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");

    public static string FormatMonth(DateOnly date) =>
        Russian.DateTimeFormat.GetMonthName(date.Month) + " " + date.Year;

    public static string FormatDay(DateOnly date) =>
        date.ToString("d MMMM yyyy, dddd", Russian);
}

internal static class DayOfWeekExtensions
{
    private static readonly CultureInfo Russian = CultureInfo.GetCultureInfo("ru-RU");

    public static string ToRussianName(this DayOfWeek day) =>
        Russian.DateTimeFormat.GetDayName(day);

    public static string ToRussianShortName(this DayOfWeek day) =>
        Russian.DateTimeFormat.GetAbbreviatedDayName(day);
}

internal static class DayOfWeekOptions
{
    public static readonly IReadOnlyList<DayOfWeek> All =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday,
        DayOfWeek.Sunday,
    ];
}
