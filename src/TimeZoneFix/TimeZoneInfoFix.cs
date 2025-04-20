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

    // TODO there may be cases where there is no next or previous adjustmentRule
    private TimeZoneInfo.AdjustmentRule GetPreviousAdjustmentRule(TimeZoneInfo.AdjustmentRule rule, int? ruleIndex) =>
        _timeZone.GetPreviousAdjustmentRule(rule, ruleIndex);
    
    private TimeZoneInfo.AdjustmentRule GetNextAdjustmentRule(TimeZoneInfo.AdjustmentRule rule, int? ruleIndex)
    {
        if (ruleIndex.HasValue && 0 < ruleIndex.GetValueOrDefault() && ruleIndex.GetValueOrDefault() < _adjustmentRules.Length)
        {
            return _adjustmentRules[ruleIndex.GetValueOrDefault() + 1];
        }

        TimeZoneInfo.AdjustmentRule result = rule;
        for (int i = 1; i < _adjustmentRules.Length; i++)
        {
            // use ReferenceEquals here instead of AdjustmentRule.Equals because
            // ReferenceEquals is much faster. This is safe because all the callers
            // of GetPreviousAdjustmentRule pass in a rule that was retrieved from
            // _adjustmentRules.  A different approach will be needed if this ever changes.
            if (ReferenceEquals(rule, _adjustmentRules[i]))
            {
                result = _adjustmentRules[i + 1];
                break;
            }
        }
        return result;
    }
}