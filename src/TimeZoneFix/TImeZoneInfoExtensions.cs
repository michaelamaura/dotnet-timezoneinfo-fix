using System.Runtime.CompilerServices;

namespace TimeZoneFix;

public static class TimeZoneInfoExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_adjustmentRules")]
    public static extern ref TimeZoneInfo.AdjustmentRule[]? AdjustmentRulesField(this TimeZoneInfo @this);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetAdjustmentRuleForTime")]
    public static extern TimeZoneInfo.AdjustmentRule? GetAdjustmentRuleForTime(this TimeZoneInfo @this,
        DateTime dateTime, out int? ruleIndex);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ConvertFromUtc")]
    public static extern DateTime
        ConvertFromUtc(this TimeZoneInfo @this, DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ConvertToFromUtc")]
    public static extern DateTime ConvertToFromUtc(this TimeZoneInfo @this, DateTime dateTime, TimeSpan daylightDelta,
        TimeSpan baseUtcOffsetDelta, bool convertToUtc);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetPreviousAdjustmentRule")]
    public static extern TimeZoneInfo.AdjustmentRule GetPreviousAdjustmentRule(this TimeZoneInfo @this,
        TimeZoneInfo.AdjustmentRule rule, int? ruleIndex);
}