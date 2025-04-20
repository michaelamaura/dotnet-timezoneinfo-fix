namespace TimeZoneFix;

public partial class TimeZoneInfoFix
{
    public bool IsInvalidTime(DateTime dateTime)
    {
        bool isInvalid = false;

        if ((dateTime.Kind == DateTimeKind.Unspecified) ||
            (dateTime.Kind == DateTimeKind.Local && s_cachedData.GetCorrespondingKind(this) == DateTimeKind.Local))
        {
            // only check Unspecified and (Local when this TimeZoneInfo instance is Local)
            TimeZoneInfo.AdjustmentRule? rule = GetAdjustmentRuleForTime(dateTime, out int? ruleIndex);

            if (rule != null && rule.HasDaylightSaving())
            {
                DaylightTimeStruct daylightTime = GetDaylightTime(dateTime.Year, rule, ruleIndex);
                isInvalid = GetIsInvalidTime(dateTime, rule, daylightTime);
            }
            else
            {
                isInvalid = false;
            }
        }

        return isInvalid;
    }

    private static bool GetIsInvalidTime(DateTime time, TimeZoneInfo.AdjustmentRule? rule,
        DaylightTimeStruct daylightTime)
    {
        bool isInvalid = false;
        if (rule == null || rule.DaylightDelta == TimeSpan.Zero)
        {
            return isInvalid;
        }

        DateTime startInvalidTime;
        DateTime endInvalidTime;

        // if at DST start we transition forward in time then there is an ambiguous time range at the DST end
        if (rule.DaylightDelta < TimeSpan.Zero)
        {
            // if the year ends with daylight saving on then there cannot be any time-hole's in that year.
            if (rule.IsEndDateMarkerForEndOfYear())
                return false;

            startInvalidTime = daylightTime.End;
            endInvalidTime = daylightTime.End - rule.DaylightDelta; /* FUTURE: + rule.StandardDelta; */
        }
        else
        {
            // if the year starts with daylight saving on then there cannot be any time-hole's in that year.
            if (rule.IsStartDateMarkerForBeginningOfYear())
                return false;

            startInvalidTime = daylightTime.Start;
            endInvalidTime = daylightTime.Start + rule.DaylightDelta; /* FUTURE: - rule.StandardDelta; */
        }

        isInvalid = (time >= startInvalidTime && time < endInvalidTime);

        if (!isInvalid && startInvalidTime.Year != endInvalidTime.Year)
        {
            // there exists an extreme corner case where the start or end period is on a year boundary and
            // because of this the comparison above might have been performed for a year-early or a year-later
            // than it should have been.
            DateTime startModifiedInvalidTime;
            DateTime endModifiedInvalidTime;
            try
            {
                startModifiedInvalidTime = startInvalidTime.AddYears(1);
                endModifiedInvalidTime = endInvalidTime.AddYears(1);
                isInvalid = (time >= startModifiedInvalidTime && time < endModifiedInvalidTime);
            }
            catch (ArgumentOutOfRangeException)
            {
            }

            if (!isInvalid)
            {
                try
                {
                    startModifiedInvalidTime = startInvalidTime.AddYears(-1);
                    endModifiedInvalidTime = endInvalidTime.AddYears(-1);
                    isInvalid = (time >= startModifiedInvalidTime && time < endModifiedInvalidTime);
                }
                catch (ArgumentOutOfRangeException)
                {
                }
            }
        }

        return isInvalid;
    }
}