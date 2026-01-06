namespace GongCalendar.Tests;

using Xunit;
using GongCalendar.Interfaces;
using GongCalendar.Models;
using GongCalendar.Services;

/// <summary>
/// Tests for caching functionality.
/// Focuses on cache behavior, performance improvements, and thread-safety.
/// </summary>
public class CachingTests
{
    private string GetTestDataPath(string filename)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "TestData", filename);
    }

    [Fact]
    public void ReadCalendarEvents_FirstCall_LoadsAndCachesData()
    {
        // Arrange
        var csvPath = GetTestDataPath("calendar.csv");
        var baseReader = new CsvCalendarDataReader(csvPath);
        var cachingReader = new CachingCalendarDataReader(baseReader);

        // Assert - Initially not cached
        Assert.False(cachingReader.IsCached);
        Assert.Equal(0, cachingReader.CachedEventCount);

        // Act
        var events = cachingReader.ReadCalendarEvents().ToList();

        // Assert - After first call, should be cached
        Assert.True(cachingReader.IsCached);
        Assert.Equal(12, cachingReader.CachedEventCount); // calendar.csv has 12 events
        Assert.Equal(12, events.Count);
    }

    [Fact]
    public void ReadCalendarEvents_SecondCall_UsesCacheNotInnerReader()
    {
        // Arrange - Create a mock reader that tracks how many times it's called
        var callCount = 0;
        var mockEvents = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting 1", new TimeOnly(8, 0), new TimeOnly(9, 0)),
            new CalendarEvent("Bob", "Meeting 2", new TimeOnly(10, 0), new TimeOnly(11, 0))
        };

        var mockReader = new MockCalendarDataReader(mockEvents, () => callCount++);
        var cachingReader = new CachingCalendarDataReader(mockReader);

        // Act - Call twice
        var firstCall = cachingReader.ReadCalendarEvents().ToList();
        var secondCall = cachingReader.ReadCalendarEvents().ToList();

        // Assert - Inner reader should only be called once
        Assert.Equal(1, callCount);
        Assert.Equal(2, firstCall.Count);
        Assert.Equal(2, secondCall.Count);

        // Both calls should return the same data
        Assert.Equal(firstCall[0].PersonName, secondCall[0].PersonName);
        Assert.Equal(firstCall[1].PersonName, secondCall[1].PersonName);
    }

    [Fact]
    public void ReadCalendarEvents_AfterClearCache_ReloadsFromSource()
    {
        // Arrange
        var callCount = 0;
        var mockEvents = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting", new TimeOnly(8, 0), new TimeOnly(9, 0))
        };

        var mockReader = new MockCalendarDataReader(mockEvents, () => callCount++);
        var cachingReader = new CachingCalendarDataReader(mockReader);

        // Act
        var firstCall = cachingReader.ReadCalendarEvents().ToList();
        Assert.Equal(1, callCount);
        Assert.True(cachingReader.IsCached);

        // Clear cache
        cachingReader.ClearCache();
        Assert.False(cachingReader.IsCached);
        Assert.Equal(0, cachingReader.CachedEventCount);

        // Call again after clearing
        var secondCall = cachingReader.ReadCalendarEvents().ToList();

        // Assert - Inner reader should be called twice (once before clear, once after)
        Assert.Equal(2, callCount);
        Assert.True(cachingReader.IsCached);
        Assert.Single(secondCall);
    }

    [Fact]
    public void ReadCalendarEvents_MultipleConcurrentCalls_OnlyLoadsOnce()
    {
        // Arrange
        var callCount = 0;
        var mockEvents = new List<CalendarEvent>
        {
            new CalendarEvent("Alice", "Meeting", new TimeOnly(8, 0), new TimeOnly(9, 0))
        };

        // Add a small delay to simulate file I/O and increase chance of race conditions
        var mockReader = new MockCalendarDataReader(mockEvents, () =>
        {
            callCount++;
            Thread.Sleep(10); // Simulate I/O delay
        });

        var cachingReader = new CachingCalendarDataReader(mockReader);

        // Act - Launch 10 threads that all try to read simultaneously
        var tasks = new List<Task<List<CalendarEvent>>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() => cachingReader.ReadCalendarEvents().ToList()));
        }

        // Wait for all threads to complete
        Task.WaitAll(tasks.ToArray());

        // Assert - Despite 10 concurrent calls, inner reader should only be called once
        Assert.Equal(1, callCount);

        // All threads should get the same data
        foreach (var task in tasks)
        {
            Assert.Single(task.Result);
            Assert.Equal("Alice", task.Result[0].PersonName);
        }
    }

    /// <summary>
    /// Mock implementation of ICalendarDataReader for testing.
    /// Tracks how many times ReadCalendarEvents is called.
    /// </summary>
    private class MockCalendarDataReader : ICalendarDataReader
    {
        private readonly List<CalendarEvent> _events;
        private readonly Action _onRead;

        public MockCalendarDataReader(List<CalendarEvent> events, Action onRead)
        {
            _events = events;
            _onRead = onRead;
        }

        public IEnumerable<CalendarEvent> ReadCalendarEvents()
        {
            _onRead(); // Increment counter
            return _events;
        }
    }
}
