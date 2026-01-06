namespace GongCalendar.Tests;

using Xunit;
using GongCalendar;
using GongCalendar.Services;

/// <summary>
/// Integration tests for CalendarScheduler - tests the complete end-to-end functionality.
/// Uses the actual calendar.csv file to verify real-world scenarios.
/// </summary>
public class SchedulerIntegrationTests
{
    private string GetTestDataPath(string filename)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "TestData", filename);
    }

    [Fact]
    public void FindAvailableSlots_AliceAndJack60Minutes_ReturnsFourSlots()
    {
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Alice", "Jack" },
            TimeSpan.FromMinutes(60)
        );

        Assert.Equal(4, slots.Count);

        Assert.Equal(new TimeOnly(7, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(7, 0), slots[0].End);

        Assert.Equal(new TimeOnly(9, 40), slots[1].Start);
        Assert.Equal(new TimeOnly(12, 0), slots[1].End);

        Assert.Equal(new TimeOnly(14, 0), slots[2].Start);
        Assert.Equal(new TimeOnly(15, 0), slots[2].End);

        Assert.Equal(new TimeOnly(17, 0), slots[3].Start);
        Assert.Equal(new TimeOnly(18, 0), slots[3].End);
    }

    [Fact]
    public void FindAvailableSlots_BobOnly30Minutes_ReturnsFourSlots()
    {
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Bob" },
            TimeSpan.FromMinutes(30)
        );

        // (09:40-10:00 is only 20 minutes, too short)
        Assert.Equal(4, slots.Count);

        Assert.Contains(slots, s => s.Start == new TimeOnly(7, 0) && s.End == new TimeOnly(7, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(11, 30) && s.End == new TimeOnly(12, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(15, 0) && s.End == new TimeOnly(15, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(17, 0) && s.End == new TimeOnly(18, 30));
    }

    [Fact]
    public void FindAvailableSlots_AllThreePeople120Minutes_ReturnsOneZeroLengthSlot()
    {
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Alice", "Jack", "Bob" },
            TimeSpan.FromMinutes(120)
        );

        Assert.Single(slots);
        Assert.Equal(new TimeOnly(17, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(17, 0), slots[0].End);
    }

    [Fact]
    public void FindAvailableSlots_WithEmptyPersonList_ReturnsEmpty()
    {
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        var slots = scheduler.FindAvailableSlots(
            new List<string>(),
            TimeSpan.FromMinutes(60)
        );

        Assert.Empty(slots);
    }
}
