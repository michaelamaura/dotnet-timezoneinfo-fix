namespace TimeZoneFix;

public static class InvalidTimeDeterminator
{
    
    // probably needs to be able to return multiple ranges... ?
    public static InvalidTimeRange[] DetermineInvalidTime(TimeZoneInfo.AdjustmentRule current,
        TimeZoneInfo.AdjustmentRule? next)
    {
        // TODO ignore next adjustment rule if it is not seamless
        
        // TODO check for invalid ranges at end of adjustmentRule

        // TODO only consider next if it's seamless

        var currentBaseUtcOffsetDelta = current.BaseUtcOffsetDelta;
        var nextBaseUtcOffsetDelta = next?.BaseUtcOffsetDelta ?? TimeSpan.Zero;

        var baseUtcOffsetDeltaChange = nextBaseUtcOffsetDelta - currentBaseUtcOffsetDelta;
        
        // TODO also take DST changes into account

        if (baseUtcOffsetDeltaChange > TimeSpan.Zero)
        {
            var endExclusive = current.DateEnd.AddDays(1);
            var formInclusive = endExclusive - baseUtcOffsetDeltaChange;

            return [new InvalidTimeRange(formInclusive, endExclusive)];
        }
        // TODO if the delta change is negative, we have an amibuous time

        return [];
    }

    public record struct InvalidTimeRange(DateTime FromInclusive, DateTime ToExclusive);
}