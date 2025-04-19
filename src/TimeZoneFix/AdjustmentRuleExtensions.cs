using System.Runtime.CompilerServices;

namespace TimeZoneFix;

public static class AdjustmentRuleExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_noDaylightTransitions")]
    public static extern ref bool NoDaylightTransitionsField(this TimeZoneInfo.AdjustmentRule @this);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_HasDaylightSaving")]
    public static extern bool HasDaylightSaving(this TimeZoneInfo.AdjustmentRule @this);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "IsStartDateMarkerForBeginningOfYear")]
    public static extern bool IsStartDateMarkerForBeginningOfYear(this TimeZoneInfo.AdjustmentRule @this);
    
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "IsEndDateMarkerForEndOfYear")]
    public static extern bool IsEndDateMarkerForEndOfYear(this TimeZoneInfo.AdjustmentRule @this);

    public static string ToFormattedString(this TimeZoneInfo.AdjustmentRule rule)
    {
        return $"DateStart: {rule.DateStart:yyyy-MM-dd}, " +
               $"DateEnd: {rule.DateEnd:yyyy-MM-dd}, " +
               $"DaylightDelta: {rule.DaylightDelta}, " +
               $"BaseUtcOffsetDelta: {rule.BaseUtcOffsetDelta}, " +
               $"NoDaylightTransitions: {NoDaylightTransitionsField(rule)}, " +
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
            return
                $"Month: {tt.Month:00}, Week: {tt.Week}, DayOfWeek: {tt.DayOfWeek}, TimeOfDay: {tt.TimeOfDay:HH:mm:ss.FFFF}";
        }
    }
}