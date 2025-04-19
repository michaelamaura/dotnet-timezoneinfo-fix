using System.Collections.ObjectModel;

namespace TimeZoneFix;

public partial class TimeZoneInfoFix
{
    [Flags]
    internal enum TimeZoneInfoOptions
    {
        None = 1,
        NoThrowOnInvalidTime = 2
    }

    public bool IsAmbiguousTime(DateTime dateTime) =>
        IsAmbiguousTime(dateTime, TimeZoneInfoOptions.NoThrowOnInvalidTime);

    internal bool IsAmbiguousTime(DateTime dateTime, TimeZoneInfoOptions flags)
    {
        if (!_supportsDaylightSavingTime)
        {
            return false;
        }

        //CachedData cachedData = s_cachedData;
        DateTime adjustedTime =
            //dateTime.Kind == DateTimeKind.Local ? ConvertTime(dateTime, cachedData.Local, this, flags, cachedData) :
            //dateTime.Kind == DateTimeKind.Utc ? ConvertTime(dateTime, s_utcTimeZone, this, flags, cachedData) :
            dateTime;

        TimeZoneInfo.AdjustmentRule? rule = GetAdjustmentRuleForTime(adjustedTime, out int? ruleIndex);
        if (rule != null && rule.HasDaylightSaving())
        {
            DaylightTimeStruct daylightTime = GetDaylightTime(adjustedTime.Year, rule, ruleIndex);
            return GetIsAmbiguousTime(adjustedTime, rule, daylightTime);
        }

        return false;
    }

    private static bool GetIsAmbiguousTime(DateTime time, TimeZoneInfo.AdjustmentRule? rule,
        DaylightTimeStruct daylightTime)
    {
        bool isAmbiguous = false;
        if (rule == null || rule.DaylightDelta == TimeSpan.Zero)
        {
            return isAmbiguous;
        }

        DateTime startAmbiguousTime;
        DateTime endAmbiguousTime;

        // if at DST start we transition forward in time then there is an ambiguous time range at the DST end
        if (rule.DaylightDelta > TimeSpan.Zero)
        {
            if (rule.IsEndDateMarkerForEndOfYear())
            {
                // year end with daylight on so there is no ambiguous time
                return false;
            }

            startAmbiguousTime = daylightTime.End;
            endAmbiguousTime = daylightTime.End - rule.DaylightDelta; /* FUTURE: + rule.StandardDelta; */
        }
        else
        {
            if (rule.IsStartDateMarkerForBeginningOfYear())
            {
                // year start with daylight on so there is no ambiguous time
                return false;
            }

            startAmbiguousTime = daylightTime.Start;
            endAmbiguousTime = daylightTime.Start + rule.DaylightDelta; /* FUTURE: - rule.StandardDelta; */
        }

        isAmbiguous = (time >= endAmbiguousTime && time < startAmbiguousTime);

        if (!isAmbiguous && startAmbiguousTime.Year != endAmbiguousTime.Year)
        {
            // there exists an extreme corner case where the start or end period is on a year boundary and
            // because of this the comparison above might have been performed for a year-early or a year-later
            // than it should have been.
            DateTime startModifiedAmbiguousTime;
            DateTime endModifiedAmbiguousTime;
            try
            {
                startModifiedAmbiguousTime = startAmbiguousTime.AddYears(1);
                endModifiedAmbiguousTime = endAmbiguousTime.AddYears(1);
                isAmbiguous = (time >= endModifiedAmbiguousTime && time < startModifiedAmbiguousTime);
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            if (!isAmbiguous)
            {
                try
                {
                    startModifiedAmbiguousTime = startAmbiguousTime.AddYears(-1);
                    endModifiedAmbiguousTime = endAmbiguousTime.AddYears(-1);
                    isAmbiguous = (time >= endModifiedAmbiguousTime && time < startModifiedAmbiguousTime);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
        }

        return isAmbiguous;
    }
}