namespace GongCalendar.Services;

using System.Globalization;
using GongCalendar.Interfaces;
using GongCalendar.Models;

/// <summary>
/// Reads calendar events from a CSV file.
/// CSV Format: PersonName,"Subject",StartTime,EndTime
/// Example: Alice,"Morning meeting",08:00,09:30
///
/// This class follows the Single Responsibility Principle (SRP) -
/// it only handles CSV parsing, nothing else.
/// </summary>
public class CsvCalendarDataReader : ICalendarDataReader
{
    private readonly string _filePath;

    /// <summary>
    /// Creates a new CSV calendar data reader
    /// </summary>
    /// <param name="filePath">Path to the CSV file</param>
    /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
    public CsvCalendarDataReader(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Calendar file not found: {filePath}");

        _filePath = filePath;
    }

    /// <summary>
    /// Reads and parses all calendar events from the CSV file
    /// </summary>
    /// <returns>Collection of calendar events</returns>
    public IEnumerable<CalendarEvent> ReadCalendarEvents()
    {
        var events = new List<CalendarEvent>();
        var lines = File.ReadAllLines(_filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            try
            {
                var calendarEvent = ParseLine(lines[i]);
                if (calendarEvent != null)
                    events.Add(calendarEvent);
            }
            catch (Exception ex)
            {
                // Log warning but continue parsing - graceful error handling
                Console.WriteLine($"Warning: Skipping line {i + 1} due to parse error: {ex.Message}");
            }
        }

        return events;
    }

    /// <summary>
    /// Parses a single CSV line into a CalendarEvent
    /// </summary>
    /// <param name="line">CSV line to parse</param>
    /// <returns>CalendarEvent or null if line is empty</returns>
    private CalendarEvent? ParseLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return null;

        // Parse CSV: Alice,"Morning meeting",08:00,09:30
        var parts = SplitCsvLine(line);

        if (parts.Length != 4)
            throw new FormatException($"Expected 4 columns, got {parts.Length}");

        var personName = parts[0].Trim();
        var subject = parts[1].Trim('"', ' '); // Remove quotes and spaces

        // Parse time format "HH:mm" (e.g., "08:00", "13:30")
        var startTime = TimeOnly.ParseExact(parts[2].Trim(), "HH:mm", CultureInfo.InvariantCulture);
        var endTime = TimeOnly.ParseExact(parts[3].Trim(), "HH:mm", CultureInfo.InvariantCulture);

        return new CalendarEvent(personName, subject, startTime, endTime);
    }

    /// <summary>
    /// Splits a CSV line into fields, handling quoted fields that may contain commas.
    /// Example: Alice,"Morning meeting, with team",08:00,09:30
    /// </summary>
    /// <param name="line">CSV line to split</param>
    /// <returns>Array of field values</returns>
    private string[] SplitCsvLine(string line)
    {
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                // Toggle quote state
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                // Comma outside quotes = field separator
                parts.Add(current.ToString());
                current.Clear();
            }
            else
            {
                // Regular character - add to current field
                current.Append(c);
            }
        }

        // Add the last field
        parts.Add(current.ToString());
        return parts.ToArray();
    }
}
