using FluentAssertions;
using Xunit.Abstractions;

namespace TimeZoneFix.Tests;

public class InvalidTimeDeterminatorTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void DetermineInvalidTime()
    {
        var ruleKiritimati1994 = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            dateStart: new DateTime(1994, 1, 1),
            dateEnd: new DateTime(1994, 12, 31),
            daylightDelta: TimeSpan.Zero,
            daylightTransitionStart: CreateFixedDateRule(TimeOnly.MinValue, 1, 1),
            daylightTransitionEnd: CreateFixedDateRule(new TimeOnly(23, 59, 59, 999), 12, 31),
            baseUtcOffsetDelta: TimeSpan.FromDays(-1));

        testOutputHelper.WriteLine(ruleKiritimati1994.ToFormattedString());

        // from inclusive, to exclusive?
        var (from, to) = InvalidTimeDeterminator.DetermineInvalidTime(ruleKiritimati1994, null);
        from.Should().Be(new DateTime(1994, 12, 31,  0, 0, 0, DateTimeKind.Unspecified));
        to.Should().Be(new DateTime(1995, 1, 1,  0, 0, 0, DateTimeKind.Unspecified));
    }

    private static TimeZoneInfo.TransitionTime CreateFixedDateRule(TimeOnly timeOfDay, int month, int day) =>
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(new DateOnly(1, 1, 1), timeOfDay), month, day);
}