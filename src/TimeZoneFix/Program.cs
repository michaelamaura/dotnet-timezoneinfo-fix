using TimeZoneFix;

var paraguayTz = TimeZoneInfo.FindSystemTimeZoneById("America/Asuncion");
var beforeInvalid = new DateTime(2024, 10, 5, 23, 59, 0);
var shouldBeInvalid = new DateTime(2024, 10, 6, 0, 0, 0);
var afterInvalid = new DateTime(2024, 10, 6, 1, 0, 0);
var clearlyAfterInvalid = new DateTime(2024, 10, 7, 0, 0, 0);
var justBeforeActualOffsetSwitch = new DateTime(2024, 10, 14, 23, 59, 59);
var afterActualOffsetSwitch = new DateTime(2024, 10, 15, 0, 0, 0);

var historicalDst = new DateTime(2024, 2, 24, 0, 0, 0);

var tzMock = new TimeZoneInfoFix(paraguayTz);

Console.WriteLine(
    $"IsInvalidTime({beforeInvalid:yyyy-MM-dd'T'HH:mm:ss}): {paraguayTz.IsInvalidTime(beforeInvalid)}, {paraguayTz.IsDaylightSavingTime(beforeInvalid)}");
Console.WriteLine(
    $"IsInvalidTime({afterInvalid:yyyy-MM-dd'T'HH:mm:ss}): {paraguayTz.IsInvalidTime(afterInvalid)}, {paraguayTz.IsDaylightSavingTime(afterInvalid)}");
Console.WriteLine(
    $"IsInvalidTime({clearlyAfterInvalid:yyyy-MM-dd'T'HH:mm:ss}): {paraguayTz.IsInvalidTime(clearlyAfterInvalid)}, {paraguayTz.IsDaylightSavingTime(clearlyAfterInvalid)}");
Console.WriteLine(
    $"IsInvalidTime({justBeforeActualOffsetSwitch:yyyy-MM-dd'T'HH:mm:ss}): {paraguayTz.IsInvalidTime(justBeforeActualOffsetSwitch)}, {paraguayTz.IsDaylightSavingTime(justBeforeActualOffsetSwitch)}");
Console.WriteLine(
    $"IsInvalidTime({afterActualOffsetSwitch:yyyy-MM-dd'T'HH:mm:ss}): {paraguayTz.IsInvalidTime(afterActualOffsetSwitch)}, {paraguayTz.IsDaylightSavingTime(afterActualOffsetSwitch)}");
Console.WriteLine(
    $"IsInvalidTime({historicalDst:yyyy-MM-dd'T'HH:mm:ss}): {paraguayTz.IsInvalidTime(historicalDst)}, {paraguayTz.IsDaylightSavingTime(historicalDst)}");

Console.WriteLine();
Console.WriteLine("---");
Console.WriteLine();

Console.WriteLine(
    $"IsInvalidTime({beforeInvalid:yyyy-MM-dd'T'HH:mm:ss}): {tzMock.IsInvalidTime(beforeInvalid)}");
Console.WriteLine();
Console.WriteLine(
    $"IsInvalidTime({afterInvalid:yyyy-MM-dd'T'HH:mm:ss}): {tzMock.IsInvalidTime(afterInvalid)}");
Console.WriteLine();
Console.WriteLine(
    $"IsInvalidTime({clearlyAfterInvalid:yyyy-MM-dd'T'HH:mm:ss}): {tzMock.IsInvalidTime(clearlyAfterInvalid)}");
Console.WriteLine();
Console.WriteLine(
    $"IsInvalidTime({afterActualOffsetSwitch:yyyy-MM-dd'T'HH:mm:ss}): {tzMock.IsInvalidTime(afterActualOffsetSwitch)}");

Console.WriteLine();
Console.WriteLine("---");
Console.WriteLine();

Console.WriteLine(
    $"IsInvalidTime({shouldBeInvalid:yyyy-MM-dd'T'HH:mm:ss}): {paraguayTz.IsInvalidTime(shouldBeInvalid)}, {paraguayTz.IsDaylightSavingTime(shouldBeInvalid)}");
Console.WriteLine();
Console.WriteLine(
    $"IsInvalidTime({shouldBeInvalid:yyyy-MM-dd'T'HH:mm:ss}): {tzMock.IsInvalidTime(shouldBeInvalid)}");

Console.WriteLine();
Console.WriteLine("---");
Console.WriteLine();

foreach (var rule in paraguayTz.GetAdjustmentRules()
             .Where(r => r.DateStart.Year is >= 2023 and <= 2025))
{
    Console.WriteLine(rule.ToFormattedString());
}

Console.WriteLine();
Console.WriteLine("---");
Console.WriteLine();

var portugalTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Lisbon");

foreach (var rule in portugalTz.GetAdjustmentRules()
             .Where(r => r.DateStart.Year is >= 1995 and <= 1997))
{
    Console.WriteLine(rule.ToFormattedString());
}

var portugalChange1995At1 = new DateTime(1995, 3, 26, 1, 0, 0);
Console.WriteLine(
    $"IsInvalidTime({portugalChange1995At1:yyyy-MM-dd'T'HH:mm:ss}): {portugalTz.IsInvalidTime(portugalChange1995At1)}");
var portugalChange1995At2 = new DateTime(1995, 3, 26, 2, 0, 0);
Console.WriteLine(
    $"IsInvalidTime({portugalChange1995At2:yyyy-MM-dd'T'HH:mm:ss}): {portugalTz.IsInvalidTime(portugalChange1995At2)}");
var portugalChange1995At3 = new DateTime(1995, 3, 26, 3, 0, 0);
Console.WriteLine(
    $"IsInvalidTime({portugalChange1995At3:yyyy-MM-dd'T'HH:mm:ss}): {portugalTz.IsInvalidTime(portugalChange1995At3)}");

var portugalChangeIn1992 = new DateTime(1992, 9, 27, 1, 30, 0);
Console.WriteLine(
    $"IsAmbiguousTime({portugalChangeIn1992:yyyy-MM-dd'T'HH:mm:ss}): {portugalTz.IsAmbiguousTime(portugalChangeIn1992)}");

Console.WriteLine();
Console.WriteLine("---");
Console.WriteLine();

var kiritimatiTz = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Kiritimati");

foreach (var rule in kiritimatiTz.GetAdjustmentRules())
{
    Console.WriteLine(rule.ToFormattedString());
}

Console.WriteLine();
Console.WriteLine("---");
Console.WriteLine();

var timeZones = TimeZoneInfo.GetSystemTimeZones().OrderBy(x => x.Id);
foreach (var timeZone in timeZones)
{
    var anyNoTransitions = timeZone.AdjustmentRulesField()?.Any(r => r.NoDaylightTransitionsField());
    Console.WriteLine($"{timeZone.Id}, {anyNoTransitions}");
}