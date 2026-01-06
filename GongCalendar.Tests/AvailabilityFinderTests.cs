namespace GongCalendar.Tests;

using Xunit;
using GongCalendar.Interfaces;
using GongCalendar.Models;
using GongCalendar.Services;

/// <summary>
/// Tests for AvailabilityFinderService - the core algorithm logic.
/// Focuses on merge logic, clipping, edge cases, and duration adjustment.
/// </summary>
public class AvailabilityFinderTests
{
    [Fact]
    public void FindAvailableSlots_WithOverlappingEvents_MergesCorrectly()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting 1", new TimeOnly(8, 0), new TimeOnly(9, 30)),
            new CalendarEvent("Alice", "Meeting 2", new TimeOnly(9, 0), new TimeOnly(10, 0)), // Overlaps with Meeting 1
            new CalendarEvent("Alice", "Meeting 3", new TimeOnly(14, 0), new TimeOnly(15, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slots = finder.FindAvailableSlots(new List<string> { "Alice" }, TimeSpan.FromMinutes(30));

        // Free periods: [07:00-08:00, 10:00-14:00, 15:00-19:00]
        // Adjusted for 30-min duration: [07:00-07:30, 10:00-13:30, 15:00-18:30]
        Assert.Equal(3, slots.Count);
        Assert.Contains(slots, s => s.Start == new TimeOnly(7, 0) && s.End == new TimeOnly(7, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(10, 0) && s.End == new TimeOnly(13, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(15, 0) && s.End == new TimeOnly(18, 30));
    }

    [Fact]
    public void FindAvailableSlots_WithEventsOutsideWorkingHours_ClipsToWorkingHours()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Bob", "Early meeting", new TimeOnly(6, 0), new TimeOnly(8, 0)), // Starts before 07:00
            new CalendarEvent("Bob", "Late meeting", new TimeOnly(18, 0), new TimeOnly(20, 0))  // Ends after 19:00
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slots = finder.FindAvailableSlots(new List<string> { "Bob" }, TimeSpan.FromMinutes(60));

        // Busy periods after clipping: [07:00-08:00, 18:00-19:00]
        // Free periods: [08:00-18:00]
        // Adjusted for 60-min duration: [08:00-17:00]
        Assert.Single(slots);
        Assert.Equal(new TimeOnly(8, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(17, 0), slots[0].End);
    }

    [Fact]
    public void FindAvailableSlots_WithUnknownPerson_TreatsAsFreeAllDay()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting", new TimeOnly(8, 0), new TimeOnly(9, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slots = finder.FindAvailableSlots(new List<string> { "Charlie" }, TimeSpan.FromMinutes(60));

        // Free period: [07:00-19:00] (12 hours = 720 minutes)
        // Adjusted for 60-min duration: [07:00-18:00]
        Assert.Single(slots);
        Assert.Equal(new TimeOnly(7, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(18, 0), slots[0].End);
    }

    [Fact]
    public void FindAvailableSlots_CaseInsensitivePersonMatching()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting 1", new TimeOnly(8, 0), new TimeOnly(9, 0)),
            new CalendarEvent("alice", "Meeting 2", new TimeOnly(10, 0), new TimeOnly(11, 0)),
            new CalendarEvent("ALICE", "Meeting 3", new TimeOnly(14, 0), new TimeOnly(15, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slotsLower = finder.FindAvailableSlots(new List<string> { "alice" }, TimeSpan.FromMinutes(30));
        var slotsUpper = finder.FindAvailableSlots(new List<string> { "ALICE" }, TimeSpan.FromMinutes(30));
        var slotsMixed = finder.FindAvailableSlots(new List<string> { "Alice" }, TimeSpan.FromMinutes(30));

        Assert.Equal(slotsLower.Count, slotsUpper.Count);
        Assert.Equal(slotsLower.Count, slotsMixed.Count);

        // Should have same busy periods regardless of case
        Assert.Equal(4, slotsLower.Count); // Free: [07:00-08:00, 09:00-10:00, 11:00-14:00, 15:00-19:00]
    }

    [Fact]
    public void FindAvailableSlots_WithAllDayBusy_ReturnsEmpty()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Bob", "All day event", new TimeOnly(7, 0), new TimeOnly(19, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slots = finder.FindAvailableSlots(new List<string> { "Bob" }, TimeSpan.FromMinutes(30));

        Assert.Empty(slots);
    }

    [Fact]
    public void FindAvailableSlots_WithZeroLengthGap_IncludesZeroLengthSlot()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Morning", new TimeOnly(7, 0), new TimeOnly(10, 0)),
            new CalendarEvent("Alice", "Afternoon", new TimeOnly(11, 0), new TimeOnly(19, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slots = finder.FindAvailableSlots(new List<string> { "Alice" }, TimeSpan.FromMinutes(60));

        // Since latestStart (10:00) == slot.Start (10:00), this is a zero-length slot [10:00-10:00]
        // Zero-length slots are now included (matches MAIN_README expected output)
        Assert.Single(slots);
        Assert.Equal(new TimeOnly(10, 0), slots[0].Start);
        Assert.Equal(new TimeOnly(10, 0), slots[0].End);
    }

    [Fact]
    public void FindAvailableSlots_DurationAdjustment_CalculatesCorrectEndTimes()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Bob", "Morning meeting", new TimeOnly(7, 0), new TimeOnly(9, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slots30 = finder.FindAvailableSlots(new List<string> { "Bob" }, TimeSpan.FromMinutes(30));
        var slots60 = finder.FindAvailableSlots(new List<string> { "Bob" }, TimeSpan.FromMinutes(60));
        var slots120 = finder.FindAvailableSlots(new List<string> { "Bob" }, TimeSpan.FromMinutes(120));

        Assert.Single(slots30);
        Assert.Equal(new TimeOnly(9, 0), slots30[0].Start);
        Assert.Equal(new TimeOnly(18, 30), slots30[0].End);

        Assert.Single(slots60);
        Assert.Equal(new TimeOnly(9, 0), slots60[0].Start);
        Assert.Equal(new TimeOnly(18, 0), slots60[0].End);

        Assert.Single(slots120);
        Assert.Equal(new TimeOnly(9, 0), slots120[0].Start);
        Assert.Equal(new TimeOnly(17, 0), slots120[0].End);
    }

    [Fact]
    public void FindAvailableSlots_WithMultiplePeople_ReturnsUnionOfBusyPeriods()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting", new TimeOnly(8, 0), new TimeOnly(9, 0)),
            new CalendarEvent("Bob", "Meeting", new TimeOnly(10, 0), new TimeOnly(11, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        var slots = finder.FindAvailableSlots(new List<string> { "Alice", "Bob" }, TimeSpan.FromMinutes(30));

        // Free periods: [07:00-08:00, 09:00-10:00, 11:00-19:00]
        // Adjusted for 30-min: [07:00-07:30, 09:00-09:30, 11:00-18:30]
        Assert.Equal(3, slots.Count);
        Assert.Contains(slots, s => s.Start == new TimeOnly(7, 0) && s.End == new TimeOnly(7, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(9, 0) && s.End == new TimeOnly(9, 30));
        Assert.Contains(slots, s => s.Start == new TimeOnly(11, 0) && s.End == new TimeOnly(18, 30));
    }

    [Fact]
    public void FindAvailableSlots_WithInvalidDuration_HandlesGracefully()
    {
        var events = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting", new TimeOnly(8, 0), new TimeOnly(9, 0))
        };

        var mockReader = new MockCalendarDataReader(events);
        var finder = new AvailabilityFinderService(mockReader);

        Assert.Throws<ArgumentException>(() =>
            finder.FindAvailableSlots(new List<string> { "Alice" }, TimeSpan.Zero));

        Assert.Throws<ArgumentException>(() =>
            finder.FindAvailableSlots(new List<string> { "Alice" }, TimeSpan.FromMinutes(-30)));

        var slots = finder.FindAvailableSlots(new List<string> { "Alice" }, TimeSpan.FromHours(15));
        Assert.Empty(slots);
    }

    /// <summary>
    /// Mock implementation of ICalendarDataReader for testing.
    /// Returns a predefined list of calendar events.
    /// </summary>
    private class MockCalendarDataReader : ICalendarDataReader
    {
        private readonly List<CalendarEvent> _events;

        public MockCalendarDataReader(List<CalendarEvent> events)
        {
            _events = events ?? new List<CalendarEvent>();
        }

        public IEnumerable<CalendarEvent> ReadCalendarEvents()
        {
            return _events;
        }
    }
}
