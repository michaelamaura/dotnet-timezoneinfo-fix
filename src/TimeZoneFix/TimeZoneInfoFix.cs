using System.Diagnostics;

namespace TimeZoneFix;

/// <summary>
/// This class mimics a TimeZoneInfo while fixing some of its behavior regarding IsInvalidTime.
/// </summary>
/// <param name="timeZone">the time zone to fix</param>
public class TimeZoneInfoFix(TimeZoneInfo timeZone)
{
    private readonly TimeZoneInfo.AdjustmentRule[]? _adjustmentRules = timeZone.GetAdjustmentRules();
    private TimeSpan BaseUtcOffset => timeZone.BaseUtcOffset;

    public bool IsInvalidTime(DateTime dateTime)
    {
        Console.WriteLine("Checking IsInvalidTime...");

        bool isInvalid = false;

        // only check Unspecified and (Local when this TimeZoneInfo instance is Local)
        TimeZoneInfo.AdjustmentRule? rule = GetAdjustmentRuleForTime(dateTime, out int? ruleIndex);

        if (rule != null && rule.HasDaylightSaving())
        {
            Console.WriteLine($"Found adjustmentRule: {rule.ToFormattedString()}");

            DaylightTimeStruct daylightTime = GetDaylightTime(dateTime.Year, rule, ruleIndex);

            Console.WriteLine($"DaylightTimeStruct: {daylightTime}");

            isInvalid = GetIsInvalidTime(dateTime, rule, daylightTime);
        }
        else
        {
            isInvalid = false;
        }

        return isInvalid;
    }

    private TimeZoneInfo.AdjustmentRule? GetAdjustmentRuleForTime(DateTime dateTime, out int? ruleIndex)
    {
        TimeZoneInfo.AdjustmentRule? result =
            GetAdjustmentRuleForTime(dateTime, dateTimeisUtc: false, ruleIndex: out ruleIndex);
        Debug.Assert(result == null || ruleIndex.HasValue,
            "If an AdjustmentRule was found, ruleIndex should also be set.");

        return result;
    }

    private TimeZoneInfo.AdjustmentRule? GetAdjustmentRuleForTime(DateTime dateTime, bool dateTimeisUtc,
        out int? ruleIndex)
    {
        if (_adjustmentRules == null || _adjustmentRules.Length == 0)
        {
            ruleIndex = null;
            return null;
        }

        // Only check the whole-date portion of the dateTime for DateTimeKind.Unspecified rules -
        // This is because the AdjustmentRule DateStart & DateEnd are stored as
        // Date-only values {4/2/2006 - 10/28/2006} but actually represent the
        // time span {4/2/2006@00:00:00.00000 - 10/28/2006@23:59:59.99999}
        DateTime date = dateTimeisUtc ? (dateTime + BaseUtcOffset).Date : dateTime.Date;

        int low = 0;
        int high = _adjustmentRules.Length - 1;

        while (low <= high)
        {
            int median = low + ((high - low) >> 1);

            TimeZoneInfo.AdjustmentRule rule = _adjustmentRules[median];
            TimeZoneInfo.AdjustmentRule previousRule = median > 0 ? _adjustmentRules[median - 1] : rule;

            int compareResult = CompareAdjustmentRuleToDateTime(rule, previousRule, dateTime, date, dateTimeisUtc);
            if (compareResult == 0)
            {
                ruleIndex = median;
                return rule;
            }
            else if (compareResult < 0)
            {
                low = median + 1;
            }
            else
            {
                high = median - 1;
            }
        }

        ruleIndex = null;
        return null;
    }

    private int CompareAdjustmentRuleToDateTime(TimeZoneInfo.AdjustmentRule rule,
        TimeZoneInfo.AdjustmentRule previousRule,
        DateTime dateTime, DateTime dateOnly, bool dateTimeisUtc)
    {
        bool isAfterStart;
        if (rule.DateStart.Kind == DateTimeKind.Utc)
        {
            DateTime dateTimeToCompare = dateTimeisUtc
                ? dateTime
                :
                // use the previous rule to compute the dateTimeToCompare, since the time daylight savings "switches"
                // is based on the previous rule's offset
                ConvertToUtc(dateTime, previousRule.DaylightDelta, previousRule.BaseUtcOffsetDelta);

            isAfterStart = dateTimeToCompare >= rule.DateStart;
        }
        else
        {
            // if the rule's DateStart is Unspecified, then use the whole-date portion
            isAfterStart = dateOnly >= rule.DateStart;
        }

        if (!isAfterStart)
        {
            return 1;
        }

        bool isBeforeEnd;
        if (rule.DateEnd.Kind == DateTimeKind.Utc)
        {
            DateTime dateTimeToCompare = dateTimeisUtc
                ? dateTime
                : ConvertToUtc(dateTime, rule.DaylightDelta, rule.BaseUtcOffsetDelta);

            isBeforeEnd = dateTimeToCompare <= rule.DateEnd;
        }
        else
        {
            // if the rule's DateEnd is Unspecified, then use the whole-date portion
            isBeforeEnd = dateOnly <= rule.DateEnd;
        }

        return isBeforeEnd ? 0 : -1;
    }

    private DateTime ConvertToUtc(DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta) =>
        ConvertToFromUtc(dateTime, daylightDelta, baseUtcOffsetDelta, convertToUtc: true);

    /// <summary>
    /// Converts the dateTime from UTC using the specified deltas.
    /// </summary>
    private DateTime ConvertFromUtc(DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta) =>
        ConvertToFromUtc(dateTime, daylightDelta, baseUtcOffsetDelta, convertToUtc: false);

    /// <summary>
    /// Converts the dateTime to or from UTC using the specified deltas.
    /// </summary>
    private DateTime ConvertToFromUtc(DateTime dateTime, TimeSpan daylightDelta, TimeSpan baseUtcOffsetDelta,
        bool convertToUtc)
    {
        Console.WriteLine($"ConvertToFromUtc(dateTime: {dateTime:yyyy-MM-dd'T'HH:mm:ss}, " +
                          $"daylightDelta: {daylightDelta},  baseUtcOffsetDelta: {baseUtcOffsetDelta}, " +
                          $"convertToUtc: {convertToUtc})");

        TimeSpan offset = BaseUtcOffset + daylightDelta + baseUtcOffsetDelta;
        if (convertToUtc)
        {
            offset = offset.Negate();
        }

        long ticks = dateTime.Ticks + offset.Ticks;

        return
            ticks > DateTime.MaxValue.Ticks ? DateTime.MaxValue :
            ticks < DateTime.MinValue.Ticks ? DateTime.MinValue :
            new DateTime(ticks);
    }

    private DaylightTimeStruct GetDaylightTime(int year, TimeZoneInfo.AdjustmentRule rule, int? ruleIndex)
    {
        TimeSpan delta = rule.DaylightDelta;
        DateTime startTime;
        DateTime endTime;
        //if (rule.NoDaylightTransitions)
        // {
        //     // NoDaylightTransitions rules don't use DaylightTransition Start and End, instead
        //     // the DateStart and DateEnd are UTC times that represent when daylight savings time changes.
        //     // Convert the UTC times into adjusted time zone times.
        //
        //     // use the previous rule to calculate the startTime, since the DST change happens w.r.t. the previous rule
        //     TimeZoneInfo.AdjustmentRule previousRule = GetPreviousAdjustmentRule(rule, ruleIndex);
        //     startTime = ConvertFromUtc(rule.DateStart, previousRule.DaylightDelta, previousRule.BaseUtcOffsetDelta);
        //
        //     endTime = ConvertFromUtc(rule.DateEnd, rule.DaylightDelta, rule.BaseUtcOffsetDelta);
        // }
        // else
        // {
        startTime = TransitionTimeToDateTime(year, rule.DaylightTransitionStart);
        endTime = TransitionTimeToDateTime(year, rule.DaylightTransitionEnd);
        // }
        return new DaylightTimeStruct(startTime, endTime, delta);
    }

    private TimeZoneInfo.AdjustmentRule GetPreviousAdjustmentRule(TimeZoneInfo.AdjustmentRule rule, int? ruleIndex)
    {
        Debug.Assert(_adjustmentRules != null);

        if (ruleIndex.HasValue && 0 < ruleIndex.GetValueOrDefault() &&
            ruleIndex.GetValueOrDefault() < _adjustmentRules.Length)
        {
            return _adjustmentRules[ruleIndex.GetValueOrDefault() - 1];
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
                result = _adjustmentRules[i - 1];
                break;
            }
        }

        return result;
    }

    private static bool GetIsInvalidTime(DateTime time, TimeZoneInfo.AdjustmentRule? rule, DaylightTimeStruct daylightTime)
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

    internal static DateTime TransitionTimeToDateTime(int year, TimeZoneInfo.TransitionTime transitionTime)
    {
        DateTime value;
        TimeSpan timeOfDay = transitionTime.TimeOfDay.TimeOfDay;

        if (transitionTime.IsFixedDateRule)
        {
            // create a DateTime from the passed in year and the properties on the transitionTime

            int day = transitionTime.Day;
            // if the day is out of range for the month then use the last day of the month
            if (day > 28)
            {
                int daysInMonth = DateTime.DaysInMonth(year, transitionTime.Month);
                if (day > daysInMonth)
                {
                    day = daysInMonth;
                }
            }

            value = new DateTime(year, transitionTime.Month, day) + timeOfDay;
        }
        else
        {
            if (transitionTime.Week <= 4)
            {
                //
                // Get the (transitionTime.Week)th Sunday.
                //
                value = new DateTime(year, transitionTime.Month, 1) + timeOfDay;

                int dayOfWeek = (int)value.DayOfWeek;
                int delta = (int)transitionTime.DayOfWeek - dayOfWeek;
                if (delta < 0)
                {
                    delta += 7;
                }

                delta += 7 * (transitionTime.Week - 1);

                if (delta > 0)
                {
                    value = value.AddDays(delta);
                }
            }
            else
            {
                //
                // If TransitionWeek is greater than 4, we will get the last week.
                //
                int daysInMonth = DateTime.DaysInMonth(year, transitionTime.Month);
                value = new DateTime(year, transitionTime.Month, daysInMonth) + timeOfDay;

                // This is the day of week for the last day of the month.
                int dayOfWeek = (int)value.DayOfWeek;
                int delta = dayOfWeek - (int)transitionTime.DayOfWeek;
                if (delta < 0)
                {
                    delta += 7;
                }

                if (delta > 0)
                {
                    value = value.AddDays(-delta);
                }
            }
        }

        return value;
    }

    public readonly struct DaylightTimeStruct(DateTime start, DateTime end, TimeSpan delta)
    {
        public readonly DateTime Start = start;
        public readonly DateTime End = end;
        public readonly TimeSpan Delta = delta;

        public override string ToString()
        {
            return $"Start: {Start:yyyy-MM-ddTHH:mm:ss}, " +
                   $"End: {End:yyyy-MM-ddTHH:mm:ss}, " +
                   $"Delta: {Delta:c}";
        }
    }
}