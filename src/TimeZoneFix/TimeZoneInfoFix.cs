namespace TimeZoneFix;

/// <summary>
/// This class mimics a TimeZoneInfo while fixing some of its behavior regarding IsInvalidTime.
/// </summary>
public partial class TimeZoneInfoFix
{
    private readonly TimeZoneInfo.AdjustmentRule[]? _adjustmentRules;
    private readonly TimeZoneInfo _timeZone;
    private readonly bool _supportsDaylightSavingTime;

    /// <summary>
    /// This class mimics a TimeZoneInfo while fixing some of its behavior regarding IsInvalidTime.
    /// </summary>
    /// <param name="timeZone">the time zone to fix</param> 
    public TimeZoneInfoFix(TimeZoneInfo timeZone)
    {
        _timeZone = timeZone;
        _adjustmentRules = timeZone.AdjustmentRulesField();
        _supportsDaylightSavingTime = timeZone.SupportsDaylightSavingTime;
    }

    private TimeSpan BaseUtcOffset => _timeZone.BaseUtcOffset;
}