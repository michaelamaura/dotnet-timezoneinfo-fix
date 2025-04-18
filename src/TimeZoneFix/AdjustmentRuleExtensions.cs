using System.Runtime.CompilerServices;

namespace TimeZoneFix;

public static class AdjustmentRuleExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_noDaylightTransitions")]
    public static extern ref bool NoDaylightTransitions(this TimeZoneInfo.AdjustmentRule @this);
    
    public static bool HasDaylightSaving(this TimeZoneInfo.AdjustmentRule rule)
    {
        return rule.DaylightDelta != TimeSpan.Zero ||
               (rule.DaylightTransitionStart != default &&
                rule.DaylightTransitionStart.TimeOfDay != DateTime.MinValue) ||
               (rule.DaylightTransitionEnd != default &&
                rule.DaylightTransitionEnd.TimeOfDay != DateTime.MinValue.AddMilliseconds(1));
    }

    public static bool IsStartDateMarkerForBeginningOfYear(this TimeZoneInfo.AdjustmentRule rule) =>
        rule.DaylightTransitionStart.Month == 1 && rule.DaylightTransitionStart.Day == 1 &&
        rule.DaylightTransitionStart.TimeOfDay.TimeOfDay.Ticks < TimeSpan.TicksPerSecond;

    public static bool IsEndDateMarkerForEndOfYear(this TimeZoneInfo.AdjustmentRule rule) =>
        rule.DaylightTransitionEnd.Month == 1 && rule.DaylightTransitionEnd.Day == 1 &&
        rule.DaylightTransitionEnd.TimeOfDay.TimeOfDay.Ticks < TimeSpan.TicksPerSecond;

    public static string ToFormattedString(this TimeZoneInfo.AdjustmentRule rule)
    {
        return $"DateStart: {rule.DateStart:yyyy-MM-dd}, " +
               $"DateEnd: {rule.DateEnd:yyyy-MM-dd}, " +
               $"DaylightDelta: {rule.DaylightDelta}, " +
               $"BaseUtcOffsetDelta: {rule.BaseUtcOffsetDelta}, " +
               $"NoDaylightTransitions: {NoDaylightTransitions(rule)}, " +
               $"DaylightTransitionStart: {TransitionTimeToString(rule.DaylightTransitionStart)}, " +
               $"DaylightTransitionEnd: {TransitionTimeToString(rule.DaylightTransitionEnd)}";
    }

    private static string TransitionTimeToString(this TimeZoneInfo.TransitionTime tt)
    {
        // For fixed-date rules
        if (tt.IsFixedDateRule)
        {
            // Example: "Month: 3, Day: 15, TimeOfDay: 02:00:00"
            return $"Month: {tt.Month:00}, Day: {tt.Day:00}, TimeOfDay: {tt.TimeOfDay:HH:mm:ss.FFFF}";
        }
        else
        {
            // Example: "Month: 3, Week: 2, DayOfWeek: Sunday, TimeOfDay: 02:00:00"
            return $"Month: {tt.Month:00}, Week: {tt.Week}, DayOfWeek: {tt.DayOfWeek}, TimeOfDay: {tt.TimeOfDay:HH:mm:ss.FFFF}";
        }
    }
}