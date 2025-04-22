using FluentAssertions;
using FluentAssertions.Execution;
using Xunit.Abstractions;

namespace TimeZoneFix.Tests;

public class InvalidTimeDeterminatorTests(ITestOutputHelper testOutputHelper)
{
    private static readonly DateOnly DateOnly = new DateOnly(1, 1, 1);

    public static
        TheoryData<TimeZoneInfo.AdjustmentRule, TimeZoneInfo.AdjustmentRule?,
            IReadOnlyCollection<InvalidTimeDeterminator.InvalidTimeRange>> InvalidTimeTestData => new()
    {
        // Kirimati, skipping 1993 to 1994 with not invalid time
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(1993, 1, 1),
                dateEnd: new DateTime(1993, 12, 31),
                daylightDelta: TimeSpan.Zero,
                baseUtcOffsetDelta: TimeSpan.FromDays(-1),
                daylightTransitionStart: CreateFixedDateRule(1, 1, TimeOnly.MinValue),
                daylightTransitionEnd: CreateFixedDateRule(12, 31, new TimeOnly(23, 59, 59, 999))),
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(1994, 1, 1),
                dateEnd: new DateTime(1994, 12, 31),
                daylightDelta: TimeSpan.Zero,
                baseUtcOffsetDelta: TimeSpan.FromDays(-1),
                daylightTransitionStart: CreateFixedDateRule(1, 1, TimeOnly.MinValue),
                daylightTransitionEnd: CreateFixedDateRule(12, 31, new TimeOnly(23, 59, 59, 999))),
            []
        },
        // Kirimati, skipping 1994-12-31 completely
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(1994, 1, 1),
                dateEnd: new DateTime(1994, 12, 31),
                daylightDelta: TimeSpan.Zero,
                baseUtcOffsetDelta: TimeSpan.FromDays(-1),
                daylightTransitionStart: CreateFixedDateRule(1, 1, TimeOnly.MinValue),
                daylightTransitionEnd: CreateFixedDateRule(12, 31, new TimeOnly(23, 59, 59, 999))),
            null,
            [
                new InvalidTimeDeterminator.InvalidTimeRange(
                    new DateTime(1994, 12, 31, 0, 0, 0, DateTimeKind.Unspecified),
                    new DateTime(1995, 1, 1, 0, 0, 0, DateTimeKind.Unspecified))
            ]
        },
        // Paraguay, last switch to DST
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(2024, 3, 24),
                dateEnd: new DateTime(2024, 10, 6),
                daylightDelta: TimeSpan.Zero,
                baseUtcOffsetDelta: TimeSpan.FromHours(-1),
                daylightTransitionStart: CreateFixedDateRule(3, 24, TimeOnly.MinValue),
                daylightTransitionEnd: CreateFixedDateRule(10, 6, new TimeOnly(0, 59, 59, 999))),
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(2024, 10, 6),
                dateEnd: new DateTime(2024, 10, 14),
                daylightDelta: TimeSpan.Zero,
                baseUtcOffsetDelta: TimeSpan.Zero,
                daylightTransitionStart: CreateFixedDateRule(10, 6, new TimeOnly(1, 0)),
                daylightTransitionEnd: CreateFixedDateRule(10, 14, new TimeOnly(23, 59, 59, 999))),
            [
                new InvalidTimeDeterminator.InvalidTimeRange(
                    new DateTime(2024, 10, 6, 23, 0, 0, DateTimeKind.Unspecified),
                    new DateTime(2024, 10, 7, 0, 0, 0, DateTimeKind.Unspecified))
            ]
        },
        // Paraguay, last adjustment rule
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(2024, 10, 6),
                dateEnd: new DateTime(2024, 10, 14),
                daylightDelta: TimeSpan.Zero,
                baseUtcOffsetDelta: TimeSpan.Zero,
                daylightTransitionStart: CreateFixedDateRule(10, 6, new TimeOnly(1, 0)),
                daylightTransitionEnd: CreateFixedDateRule(10, 14, new TimeOnly(23, 59, 59, 999))),
            null,
            []
        },
        // Portugal 1995 - this rule looks wrong, maybe there is also an issue with the parsing?
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(1995, 1, 1),
                dateEnd: new DateTime(1995, 3, 26),
                daylightDelta: TimeSpan.Zero,
                baseUtcOffsetDelta: TimeSpan.FromHours(1),
                daylightTransitionStart: CreateFixedDateRule(1, 1, TimeOnly.MinValue),
                daylightTransitionEnd: CreateFixedDateRule(3, 26, new TimeOnly(0, 59, 59, 999))),
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(1995, 3, 26),
                dateEnd: new DateTime(1995, 9, 24),
                daylightDelta: TimeSpan.FromHours(2),
                baseUtcOffsetDelta: TimeSpan.Zero,
                daylightTransitionStart: CreateFixedDateRule(3, 26, new TimeOnly(1, 0)),
                daylightTransitionEnd: CreateFixedDateRule(9, 24, new TimeOnly(2, 59, 59, 999))),
            [
                new InvalidTimeDeterminator.InvalidTimeRange(
                    new DateTime(1995, 3, 26, 1, 0, 0, DateTimeKind.Unspecified),
                    new DateTime(1995, 3, 26, 2, 0, 0, DateTimeKind.Unspecified))
            ]
        },
        // Berlin 2024
        {
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(2024, 3, 31),
                dateEnd: new DateTime(2024, 10, 27),
                daylightDelta: TimeSpan.FromHours(1),
                baseUtcOffsetDelta: TimeSpan.Zero,
                daylightTransitionStart: CreateFixedDateRule(3, 21, new TimeOnly(2, 0)),
                daylightTransitionEnd: CreateFixedDateRule(10, 27, new TimeOnly(2, 59, 59, 999))),
            TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                dateStart: new DateTime(2025, 3, 30),
                dateEnd: new DateTime(2025, 10, 26),
                daylightDelta: TimeSpan.FromHours(1),
                baseUtcOffsetDelta: TimeSpan.Zero,
                daylightTransitionStart: CreateFixedDateRule(3, 30, new TimeOnly(1, 0)),
                daylightTransitionEnd: CreateFixedDateRule(10, 26, new TimeOnly(2, 59, 59, 999))),
            [
                new InvalidTimeDeterminator.InvalidTimeRange(
                    new DateTime(1995, 3, 26, 1, 0, 0, DateTimeKind.Unspecified),
                    new DateTime(1995, 3, 26, 2, 0, 0, DateTimeKind.Unspecified))
            ]
        },
    };


    [Theory]
    [MemberData(nameof(InvalidTimeTestData))]
    public void DetermineInvalidTime(TimeZoneInfo.AdjustmentRule current, TimeZoneInfo.AdjustmentRule? next,
        IReadOnlyCollection<InvalidTimeDeterminator.InvalidTimeRange> expected)
    {
        testOutputHelper.WriteLine(current.ToFormattedString());

        // from inclusive, to exclusive?
        var invalidInvalidTimes = InvalidTimeDeterminator.DetermineInvalidTime(current, next);

        using var _ = new AssertionScope();
        invalidInvalidTimes.Should().BeEquivalentTo(expected);
    }

    private static TimeZoneInfo.TransitionTime CreateFixedDateRule(int month, int day, TimeOnly timeOfDay) =>
        TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(DateOnly, timeOfDay), month, day);
}