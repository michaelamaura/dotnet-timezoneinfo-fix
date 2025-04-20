using FluentAssertions;
using NodaTime;

namespace TimeZoneFix.Tests;

public class TimeZoneInfoTests
{
    public static TheoryData<DateTime, string, bool> InvalidTimeTestData => new()
    {
        // Paraguay DST start (invalid time)
        { new DateTime(2024, 10, 6, 0, 0, 0), "America/Asuncion", true },
        // Berlin DST start (invalid time)
        { new DateTime(2025, 3, 30, 2, 30, 0), "Europe/Berlin", true },
        // Lisbon DST start (invalid time)
        { new DateTime(2025, 3, 30, 1, 30, 0), "Europe/Lisbon", true },
        // London DST start (invalid time)
        { new DateTime(2025, 3, 30, 1, 30, 0), "Europe/London", true },
        // Valid time after DST transition in Paraguay
        { new DateTime(2024, 10, 6, 1, 0, 0), "America/Asuncion", false },
        // Valid time in Berlin
        { new DateTime(2025, 4, 19, 15, 0, 0), "Europe/Berlin", false },
        // should not be invalid, but BCL DateTimeZone detects it to be invalid
        { new DateTime(1995, 3, 26, 3, 0, 0), "Europe/Lisbon", false },
        // Kiritimati skipped the whole day of Dec 31st in 1994 due to an offset change
        { new DateTime(1994, 12, 31, 0, 0, 0), "Pacific/Kiritimati", true },
        { new DateTime(1994, 12, 31, 12, 0, 0), "Pacific/Kiritimati", true },
        { new DateTime(1994, 12, 31, 12, 23, 59), "Pacific/Kiritimati", true },
        // Moscow switched to UTC+4 in 2011
        { new DateTime(2011, 3, 27, 2, 0, 0), "Europe/Moscow", true },
    };

    public static TheoryData<DateTime, string, bool> AmbiguousTimeTestData => new()
    {
        // DST end in Berlin (ambiguous)
        { new DateTime(2023, 10, 29, 2, 30, 0), "Europe/Berlin", true },
        // DST end in London (ambiguous)
        { new DateTime(2023, 10, 29, 1, 30, 0), "Europe/London", true },
        // DST end in Lisbon (ambiguous)
        { new DateTime(2023, 10, 29, 1, 30, 0), "Europe/Lisbon", true },
        // DST end in Lisbon (not ambiguous due to time zone switch at the same time)
        { new DateTime(1992, 9, 27, 1, 30, 0), "Europe/Lisbon", false },
        // After DST transition in Berlin (not ambiguous)
        { new DateTime(2023, 10, 29, 3, 0, 0), "Europe/Berlin", false },
        // DST end in New York (ambiguous)
        { new DateTime(2023, 11, 5, 1, 30, 0), "America/New_York", true },
        // Normal DST time in Berlin (not ambiguous)
        { new DateTime(2023, 7, 15, 12, 0, 0), "Europe/Berlin", false },
        // Moscow switched to UTC+3 in 2014
        { new DateTime(2014, 10, 26, 1, 0, 0), "Europe/Moscow", true },
    };

    [Theory]
    [MemberData(nameof(InvalidTimeTestData))]
    public void IsInvalidTime_WithBclTimeZoneInfo(
        DateTime testTime, string timeZoneId, bool expectedIsInvalid)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        timeZone.IsInvalidTime(testTime).Should().Be(expectedIsInvalid);
    }

    [Theory]
    [MemberData(nameof(InvalidTimeTestData))]
    public void IsInvalidTime_WithTimeZoneInfoFix(
        DateTime testTime, string timeZoneId, bool expectedIsInvalid)
    {
        var timeZone = new TimeZoneInfoFix(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        timeZone.IsInvalidTime(testTime).Should().Be(expectedIsInvalid);
    }

    [Theory]
    [MemberData(nameof(InvalidTimeTestData))]
    public void IsInvalidTime_WithNodaDateTimeZone_WithTzDb(
        DateTime testTime, string timeZoneId, bool expectedIsInvalid)
    {
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];
        timeZone.MapLocal(LocalDateTime.FromDateTime(testTime)).Count.Should().Be(expectedIsInvalid ? 0 : 1);
    }

    [Theory]
    [MemberData(nameof(InvalidTimeTestData))]
    public void IsInvalidTime_WithNodaDateTimeZone_WithBcl(
        DateTime testTime, string timeZoneId, bool expectedIsInvalid)
    {
        var timeZone = DateTimeZoneProviders.Bcl[timeZoneId];
        timeZone.MapLocal(LocalDateTime.FromDateTime(testTime)).Count.Should().Be(expectedIsInvalid ? 0 : 1);
    }

    [Theory]
    [MemberData(nameof(AmbiguousTimeTestData))]
    public void IsAmbiguousTime_WithBclTimeZoneInfo(
        DateTime testTime, string timeZoneId, bool expectedIsAmbiguous)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        timeZone.IsAmbiguousTime(testTime).Should().Be(expectedIsAmbiguous);
    }

    [Theory]
    [MemberData(nameof(AmbiguousTimeTestData))]
    public void IsAmbiguousTime_WithTimeZoneInfoFix(
        DateTime testTime, string timeZoneId, bool expectedIsAmbiguous)
    {
        var timeZone = new TimeZoneInfoFix(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
        timeZone.IsAmbiguousTime(testTime).Should().Be(expectedIsAmbiguous);
    }

    [Theory]
    [MemberData(nameof(AmbiguousTimeTestData))]
    public void IsAmbiguousTime_WithNodaDateTimeZone_WithTzDb(
        DateTime testTime, string timeZoneId, bool expectedIsAmbiguous)
    {
        var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];
        timeZone.MapLocal(LocalDateTime.FromDateTime(testTime)).Count.Should().Be(expectedIsAmbiguous ? 2 : 1);
    }

    [Theory]
    [MemberData(nameof(AmbiguousTimeTestData))]
    public void IsAmbiguousTime_WithNodaDateTimeZone_WithBcl(
        DateTime testTime, string timeZoneId, bool expectedIsAmbiguous)
    {
        var timeZone = DateTimeZoneProviders.Bcl[timeZoneId];
        timeZone.MapLocal(LocalDateTime.FromDateTime(testTime)).Count.Should().Be(expectedIsAmbiguous ? 2 : 1);
    }
}