namespace GongCalendar.Tests;

using Xunit;
using GongCalendar.Services;

/// <summary>
/// Tests for CSV parsing functionality.
/// Focuses on edge cases, malformed data handling, and streaming behavior.
/// </summary>
public class CsvParsingTests
{
    private string GetTestDataPath(string filename)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(baseDir, "TestData", filename);
    }

    [Fact]
    public void ReadCalendarEvents_WithQuotedSubjectContainingComma_ParsesCorrectly()
    {
        // Arrange
        var csvPath = GetTestDataPath("quoted_comma.csv");
        var reader = new CsvCalendarDataReader(csvPath);

        // Act
        var events = reader.ReadCalendarEvents().ToList();

        // Assert
        Assert.Equal(2, events.Count);
        Assert.Equal("Meeting, with Bob", events[0].Subject);
        Assert.Equal("Team sync, important", events[1].Subject);
    }

    [Fact]
    public void ReadCalendarEvents_WithMalformedLines_SkipsInvalidKeepsValid()
    {
        // Arrange
        var csvPath = GetTestDataPath("malformed.csv");
        var reader = new CsvCalendarDataReader(csvPath);

        // Act
        var events = reader.ReadCalendarEvents().ToList();

        // Assert - Should parse 3 valid lines, skip 2 invalid
        // Valid: Alice "Morning meeting", Jack "Lunch", Charlie "Valid meeting"
        // Invalid: Bob (bad time format "8:00am"), Jack "Sales call" (missing end time)
        Assert.Equal(3, events.Count);
        Assert.Contains(events, e => e.PersonName == "Alice" && e.Subject == "Morning meeting");
        Assert.Contains(events, e => e.PersonName == "Jack" && e.Subject == "Lunch");
        Assert.Contains(events, e => e.PersonName == "Charlie" && e.Subject == "Valid meeting");
    }

    [Fact]
    public void ReadCalendarEvents_WithInvalidTimeFormat_SkipsLineAndContinues()
    {
        // Arrange
        var csvPath = GetTestDataPath("malformed.csv");
        var reader = new CsvCalendarDataReader(csvPath);

        // Act
        var events = reader.ReadCalendarEvents().ToList();

        // Assert - Should not throw exception, should skip invalid time format
        Assert.DoesNotContain(events, e => e.PersonName == "Bob");
    }

    [Fact]
    public void ReadCalendarEvents_WithTooFewColumns_SkipsLine()
    {
        // Arrange
        var csvPath = GetTestDataPath("malformed.csv");
        var reader = new CsvCalendarDataReader(csvPath);

        // Act
        var events = reader.ReadCalendarEvents().ToList();

        // Assert - Line with "Sales call" has only 3 columns, should be skipped
        Assert.DoesNotContain(events, e => e.Subject == "Sales call");
    }

    [Fact]
    public void ReadCalendarEvents_WithEmptyFile_ReturnsEmptyEnumerable()
    {
        // Arrange
        var csvPath = GetTestDataPath("empty.csv");
        var reader = new CsvCalendarDataReader(csvPath);

        // Act
        var events = reader.ReadCalendarEvents().ToList();

        // Assert
        Assert.Empty(events);
    }

    [Fact]
    public void ReadCalendarEvents_IsLazyEvaluated_DoesNotReadFileUntilEnumerated()
    {
        // Arrange
        var csvPath = GetTestDataPath("calendar.csv");
        var reader = new CsvCalendarDataReader(csvPath);

        // Act - Call method but don't enumerate
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var events = reader.ReadCalendarEvents();
        stopwatch.Stop();

        // Assert - Should return almost instantly (< 10ms) because no file reading yet
        Assert.True(stopwatch.ElapsedMilliseconds < 10,
            $"Method took {stopwatch.ElapsedMilliseconds}ms, expected < 10ms for lazy evaluation");

        // Now actually enumerate to verify it works
        var eventList = events.ToList();
        Assert.Equal(12, eventList.Count); // Verify data is correct when enumerated
    }

    [Fact]
    public void ReadCalendarEvents_WithMixedCasePersonNames_ParsesCorrectly()
    {
        // Arrange
        var csvPath = GetTestDataPath("calendar.csv");
        var reader = new CsvCalendarDataReader(csvPath);

        // Act
        var events = reader.ReadCalendarEvents().ToList();

        // Assert - Person names should be preserved as-is from CSV
        Assert.Contains(events, e => e.PersonName == "Alice");
        Assert.Contains(events, e => e.PersonName == "Jack");
        Assert.Contains(events, e => e.PersonName == "Bob");

        // All should be exactly as written, not normalized
        Assert.All(events, e => Assert.Equal(e.PersonName, e.PersonName.Trim()));
    }
}
