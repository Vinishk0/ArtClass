namespace ArtClass.Domain.Services;

public static class ScheduleCycle
{
    public const int WeeksInCycle = 2;

    public static int GetCycleWeek(DateOnly date, DateOnly cycleStartDate)
    {
        var daysSinceStart = date.DayNumber - cycleStartDate.DayNumber;
        if (daysSinceStart < 0)
        {
            daysSinceStart = ((daysSinceStart % (WeeksInCycle * 7)) + WeeksInCycle * 7) % (WeeksInCycle * 7);
        }

        return (daysSinceStart / 7) % WeeksInCycle + 1;
    }

    public static DateOnly GetMondayOfWeek(DateOnly date)
    {
        var offset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-offset);
    }
}
