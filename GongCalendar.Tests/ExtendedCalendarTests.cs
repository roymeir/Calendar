namespace GongCalendar.Tests;

using Xunit;
using GongCalendar;
using GongCalendar.Services;

/// <summary>
/// Tests for the extended calendar with 12 employees.
/// Validates that the scheduler works correctly with more complex, realistic scenarios.
/// </summary>
public class ExtendedCalendarTests
{
    private string GetTestDataPath(string filename)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "TestData", filename);
    }

    [Fact]
    public void ExtendedCalendar_AliceAndJack30Minutes_ReturnsThreeSlots()
    {
        var csvPath = GetTestDataPath("extendedCalendar.csv");

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Extended calendar not found at: {csvPath}");
        }

        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Alice", "Jack" },
            TimeSpan.FromMinutes(30)
        );

        Assert.Equal(3, slots.Count);

        Assert.Equal(new TimeOnly(11, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(11, 30), slots[0].End);

        Assert.Equal(new TimeOnly(17, 0), slots[1].Start);
        Assert.Equal(new TimeOnly(17, 0), slots[1].End);

        Assert.Equal(new TimeOnly(18, 30), slots[2].Start);
        Assert.Equal(new TimeOnly(18, 30), slots[2].End);
    }

    [Fact]
    public void ExtendedCalendar_LoadsAllEvents()
    {
        var csvPath = GetTestDataPath("extendedCalendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);

        var events = dataReader.ReadCalendarEvents().ToList();

        Assert.Equal(83, events.Count);

        var uniquePeople = events.Select(e => e.PersonName).Distinct().OrderBy(n => n).ToList();
        Assert.Equal(12, uniquePeople.Count);
        Assert.Contains("Alice", uniquePeople);
        Assert.Contains("Bob", uniquePeople);
        Assert.Contains("Charlie", uniquePeople);
        Assert.Contains("Diana", uniquePeople);
        Assert.Contains("Eve", uniquePeople);
        Assert.Contains("Frank", uniquePeople);
        Assert.Contains("Grace", uniquePeople);
        Assert.Contains("Henry", uniquePeople);
        Assert.Contains("Iris", uniquePeople);
        Assert.Contains("Jack", uniquePeople);
        Assert.Contains("Kevin", uniquePeople);
        Assert.Contains("Laura", uniquePeople);
    }

    [Fact]
    public void ExtendedCalendar_FivePeople60Minutes_VeryConstrained()
    {
        var csvPath = GetTestDataPath("extendedCalendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Alice", "Jack", "Bob", "Charlie", "Diana" },
            TimeSpan.FromMinutes(60)
        );

        // We expect very few slots (likely 0-2)
        Assert.True(slots.Count <= 3,
            $"Expected at most 3 slots for 5 people, but found {slots.Count}");

        // All slots should be valid (Start <= End)
        foreach (var slot in slots)
        {
            Assert.True(slot.Start <= slot.End,
                $"Invalid slot: {slot.Start} > {slot.End}");
        }
    }

    [Fact]
    public void ExtendedCalendar_AllTwelvePeople15Minutes_ExtremelyConstrained()
    {
        var csvPath = GetTestDataPath("extendedCalendar.csv");
        var dataReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(dataReader);
        var finder = new AvailabilityFinderService(cachingReader);
        var scheduler = new CalendarScheduler(finder);

        var slots = scheduler.FindAvailableSlots(
            new List<string> { "Alice", "Jack", "Bob", "Charlie", "Diana", "Eve",
                              "Frank", "Grace", "Henry", "Iris", "Kevin", "Laura" },
            TimeSpan.FromMinutes(15)
        );

        Assert.True(slots.Count <= 2,
            $"Expected at most 2 slots for 12 people, but found {slots.Count}");

        foreach (var slot in slots)
        {
            Assert.True(slot.Start >= new TimeOnly(7, 0), "Slot starts before working hours");
            Assert.True(slot.End <= new TimeOnly(19, 0), "Slot ends after working hours");
        }
    }
}
