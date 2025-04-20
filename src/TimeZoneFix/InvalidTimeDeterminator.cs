namespace TimeZoneFix;

public static class InvalidTimeDeterminator
{
    // probably needs to be able to return multiple ranges... ?
    public static (DateTime From, DateTime To)[] DetermineInvalidTime(TimeZoneInfo.AdjustmentRule current,
        TimeZoneInfo.AdjustmentRule? next)
    {
        // TODO check for invalid ranges at end of adjustmentRule

        // TODO only consider next if it's seamless

        var nextBaseUtcOffsetDelta = next?.BaseUtcOffsetDelta ?? TimeSpan.Zero;
        var currentBaseUtcOffsetDelta = current.BaseUtcOffsetDelta;

        var baseUtcOffsetDeltaChange = nextBaseUtcOffsetDelta - currentBaseUtcOffsetDelta;

        if (baseUtcOffsetDeltaChange > TimeSpan.Zero)
        {
            var endExclusive = current.DateEnd.AddDays(1);
            var from = endExclusive - baseUtcOffsetDeltaChange;
            var to = endExclusive;

            return [(from, to)];
        }

        return [];
    }
}