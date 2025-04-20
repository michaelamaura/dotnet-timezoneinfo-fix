namespace TimeZoneFix;

public partial class TimeZoneInfoFix
{
    private CachedData s_cachedData = null!;
    private static readonly TimeZoneInfo s_utcTimeZone = null!;

    private DaylightTimeStruct GetDaylightTime(int year, TimeZoneInfo.AdjustmentRule rule, int? ruleIndex)
    {
        var delta = rule.DaylightDelta;
        DateTime startTime;
        DateTime endTime;
        if (rule.NoDaylightTransitionsField())
        {
            // NoDaylightTransitions rules don't use DaylightTransition Start and End, instead
            // the DateStart and DateEnd are UTC times that represent when daylight savings time changes.
            // Convert the UTC times into adjusted time zone times.

            // use the previous rule to calculate the startTime, since the DST change happens w.r.t. the previous rule
            var previousRule = GetPreviousAdjustmentRule(rule, ruleIndex);
            startTime = ConvertFromUtc(rule.DateStart, previousRule.DaylightDelta, previousRule.BaseUtcOffsetDelta);

            endTime = ConvertFromUtc(rule.DateEnd, rule.DaylightDelta, rule.BaseUtcOffsetDelta);
        }
        else
        {
            startTime = TransitionTimeToDateTime(year, rule.DaylightTransitionStart);
            endTime = TransitionTimeToDateTime(year, rule.DaylightTransitionEnd);
        }

        return new DaylightTimeStruct(startTime, endTime, delta);
    }


    internal static DateTime TransitionTimeToDateTime(int year, TimeZoneInfo.TransitionTime transitionTime)
    {
        DateTime value;
        var timeOfDay = transitionTime.TimeOfDay.TimeOfDay;

        if (transitionTime.IsFixedDateRule)
        {
            // create a DateTime from the passed in year and the properties on the transitionTime

            var day = transitionTime.Day;
            // if the day is out of range for the month then use the last day of the month
            if (day > 28)
            {
                var daysInMonth = DateTime.DaysInMonth(year, transitionTime.Month);
                if (day > daysInMonth) day = daysInMonth;
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

                var dayOfWeek = (int)value.DayOfWeek;
                var delta = (int)transitionTime.DayOfWeek - dayOfWeek;
                if (delta < 0) delta += 7;

                delta += 7 * (transitionTime.Week - 1);

                if (delta > 0) value = value.AddDays(delta);
            }
            else
            {
                //
                // If TransitionWeek is greater than 4, we will get the last week.
                //
                var daysInMonth = DateTime.DaysInMonth(year, transitionTime.Month);
                value = new DateTime(year, transitionTime.Month, daysInMonth) + timeOfDay;

                // This is the day of week for the last day of the month.
                var dayOfWeek = (int)value.DayOfWeek;
                var delta = dayOfWeek - (int)transitionTime.DayOfWeek;
                if (delta < 0) delta += 7;

                if (delta > 0) value = value.AddDays(-delta);
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

    private static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo sourceTimeZone,
        TimeZoneInfoFix destinationTimeZone, TimeZoneInfoOptions flags, CachedData cachedData) =>
        throw new NotImplementedException();

    private sealed class CachedData
    {
        public DateTimeKind GetCorrespondingKind(TimeZoneInfoFix timeZone) => throw new NotImplementedException();
        public TimeZoneInfo Local => throw new NotImplementedException();
    }
}