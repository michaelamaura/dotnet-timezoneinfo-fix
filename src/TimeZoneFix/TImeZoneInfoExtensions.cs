using System.Runtime.CompilerServices;

namespace TimeZoneFix;

public static class TimeZoneInfoExtensions
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_adjustmentRules")]
    public static extern ref TimeZoneInfo.AdjustmentRule[]? AdjustmentRulesField(this TimeZoneInfo @this);
}