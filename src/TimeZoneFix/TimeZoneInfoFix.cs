namespace TimeZoneFix;

/// <summary>
/// This class mimics a TimeZoneInfo while fixing some of its behavior regarding IsInvalidTime.
/// </summary>
public partial class TimeZoneInfoFix
{
    private readonly TimeZoneInfo _timeZone;
    private readonly TimeZoneInfo.AdjustmentRule[] _adjustmentRules;
    private readonly bool _supportsDaylightSavingTime;

    /// <summary>
    /// This class mimics a TimeZoneInfo while fixing some of its behavior regarding IsInvalidTime.
    /// </summary>
    /// <param name="timeZone">the time zone to fix</param> 
    public TimeZoneInfoFix(TimeZoneInfo timeZone)
    {
        _timeZone = timeZone;
        _adjustmentRules = timeZone.AdjustmentRulesField() ?? throw new NullReferenceException();
        _supportsDaylightSavingTime = timeZone.SupportsDaylightSavingTime;
    }

    private TimeZoneInfo.AdjustmentRule? GetAdjustmentRuleForTime(DateTime dateTime, out int? ruleIndex) =>
        _timeZone.GetAdjustmentRuleForTime(dateTime, out ruleIndex);

    private DateTime ConvertFromUtc(DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta) =>
        _timeZone.ConvertFromUtc(dateTime, daylightDelta, baseUtcOffsetDelta);

    private TimeZoneInfo.AdjustmentRule GetPreviousAdjustmentRule(TimeZoneInfo.AdjustmentRule rule, int? ruleIndex) =>
        _timeZone.GetPreviousAdjustmentRule(rule, ruleIndex);
}