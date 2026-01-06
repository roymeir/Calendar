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
        // Arrange - This is the PRIMARY ACCEPTANCE TEST from requirements
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        // Act - Find slots for Alice & Jack with 60-minute meeting
        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Alice", "Jack" },
            TimeSpan.FromMinutes(60)
        );

        // Assert - Expected results based on calendar.csv (matches MAIN_README):
        // Alice busy: [08:00-09:30, 13:00-14:00, 16:00-17:00]
        // Jack busy: [08:00-08:50, 09:00-09:40, 13:00-14:00, 16:00-17:00]
        // Combined & merged: [08:00-09:40, 13:00-14:00, 16:00-17:00]
        // Free: [07:00-08:00, 09:40-13:00, 14:00-16:00, 17:00-19:00]
        // Adjusted for 60-min (including zero-length): [07:00-07:00, 09:40-12:00, 14:00-15:00, 17:00-18:00]
        Assert.Equal(4, slots.Count);

        // First slot: 07:00-07:00 (zero-length - exactly one start time)
        Assert.Equal(new TimeOnly(7, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(7, 0), slots[0].End);

        // Second slot: 09:40-12:00
        Assert.Equal(new TimeOnly(9, 40), slots[1].Start);
        Assert.Equal(new TimeOnly(12, 0), slots[1].End);

        // Third slot: 14:00-15:00
        Assert.Equal(new TimeOnly(14, 0), slots[2].Start);
        Assert.Equal(new TimeOnly(15, 0), slots[2].End);

        // Fourth slot: 17:00-18:00
        Assert.Equal(new TimeOnly(17, 0), slots[3].Start);
        Assert.Equal(new TimeOnly(18, 0), slots[3].End);
    }

    [Fact]
    public void FindAvailableSlots_BobOnly30Minutes_ReturnsFourSlots()
    {
        // Arrange
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        // Act - Find slots for Bob with 30-minute meeting
        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Bob" },
            TimeSpan.FromMinutes(30)
        );

        // Assert - Expected results based on calendar.csv:
        // Bob busy: [08:00-09:30, 09:30-09:40, 10:00-11:30, 13:00-15:00, 16:00-17:00]
        // Merged: [08:00-09:40, 10:00-11:30, 13:00-15:00, 16:00-17:00]
        // Free: [07:00-08:00, 09:40-10:00, 11:30-13:00, 15:00-16:00, 17:00-19:00]
        // For 30-min: [07:00-07:30, 11:30-12:30, 15:00-15:30, 17:00-18:30]
        // (09:40-10:00 is only 20 minutes, too short)
        Assert.Equal(4, slots.Count);

        // Verify each slot
        Assert.Contains(slots, s => s.Start == new TimeOnly(7, 0) && s.End == new TimeOnly(7, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(11, 30) && s.End == new TimeOnly(12, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(15, 0) && s.End == new TimeOnly(15, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(17, 0) && s.End == new TimeOnly(18, 30));
    }

    [Fact]
    public void FindAvailableSlots_AllThreePeople120Minutes_ReturnsOneZeroLengthSlot()
    {
        // Arrange - COMPLEX SCENARIO: All three people with long meeting
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        // Act - Find slots for Alice, Jack, and Bob with 120-minute meeting
        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Alice", "Jack", "Bob" },
            TimeSpan.FromMinutes(120)
        );

        // Assert - Expected: Very limited availability
        // Combined busy periods: [08:00-09:40, 10:00-11:30, 13:00-15:00, 16:00-17:00]
        // Free: [07:00-08:00 (60min), 09:40-10:00 (20min), 11:30-13:00 (90min), 15:00-16:00 (60min), 17:00-19:00 (120min)]
        // For 120-min meeting, only [17:00-19:00] is long enough:
        // Latest start = 19:00 - 120min = 17:00 → [17:00-17:00] is zero-length, now included
        Assert.Single(slots);
        Assert.Equal(new TimeOnly(17, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(17, 0), slots[0].End);
    }

    [Fact]
    public void FindAvailableSlots_WithEmptyPersonList_ReturnsEmpty()
    {
        // Arrange
        var csvPath = GetTestDataPath("calendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        // Act - Find slots with empty person list
        var slots = scheduler.FindAvailableSlots(
            new List<string>(),
            TimeSpan.FromMinutes(60)
        );

        // Assert - Empty person list should return no slots
        Assert.Empty(slots);
    }
}
