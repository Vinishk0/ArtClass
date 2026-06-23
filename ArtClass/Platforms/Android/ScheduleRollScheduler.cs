using Android.App;
using Android.Content;
using Android.OS;
using ArtClass.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ArtClass.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class ScheduleRollReceiver : BroadcastReceiver
{
    public const string ActionRoll = "com.vinishk0.artclass.SCHEDULE_ROLL";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent?.Action != ActionRoll)
        {
            return;
        }

        var services = IPlatformApplication.Current?.Services;
        if (services is null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var rollService = scope.ServiceProvider.GetRequiredService<IScheduleRollService>();
                await rollService.RollAsync();
            }
            catch
            {
                // Background roll — ignore UI-less failures
            }
        });
    }
}

internal static class ScheduleRollScheduler
{
    public static void Schedule()
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager?)context.GetSystemService(Context.AlarmService);
        if (alarmManager is null)
        {
            return;
        }

        var intent = new Intent(context, typeof(ScheduleRollReceiver));
        intent.SetAction(ScheduleRollReceiver.ActionRoll);

        var pending = PendingIntent.GetBroadcast(
            context,
            0,
            intent,
            PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);
        if (pending is null)
        {
            return;
        }

        var trigger = GetNextSundayUtc();

        alarmManager.SetRepeating(
            AlarmType.RtcWakeup,
            trigger,
            7 * 24 * 60 * 60 * 1000L,
            pending);
    }

    private static long GetNextSundayUtc()
    {
        var now = DateTimeOffset.UtcNow;
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)now.DayOfWeek + 7) % 7;
        if (daysUntilSunday == 0 && now.Hour >= 23)
        {
            daysUntilSunday = 7;
        }

        var nextSunday = now.Date.AddDays(daysUntilSunday).AddHours(23);
        return new DateTimeOffset(nextSunday, TimeSpan.Zero).ToUnixTimeMilliseconds();
    }
}
