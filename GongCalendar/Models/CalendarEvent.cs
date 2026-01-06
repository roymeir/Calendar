namespace GongCalendar.Models;

/// <summary>
/// Represents a calendar event for a specific person.
/// This is a domain model that encapsulates a single busy period in someone's calendar.
/// </summary>
public class CalendarEvent : IComparable<CalendarEvent>
{
    /// <summary>
    /// Name of the person this event belongs to
    /// </summary>
    public string PersonName { get; init; }

    /// <summary>
    /// Subject/title of the calendar event
    /// </summary>
    public string Subject { get; init; }

    /// <summary>
    /// Start time of the event (time-only, no date)
    /// </summary>
    public TimeOnly StartTime { get; init; }

    /// <summary>
    /// End time of the event (time-only, no date)
    /// </summary>
    public TimeOnly EndTime { get; init; }

    /// <summary>
    /// Creates a new calendar event with validation
    /// </summary>
    /// <param name="personName">Name of the person (required)</param>
    /// <param name="subject">Event subject/title</param>
    /// <param name="startTime">Event start time</param>
    /// <param name="endTime">Event end time (must be after start time)</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public CalendarEvent(string personName, string subject, TimeOnly startTime, TimeOnly endTime)
    {
        if (string.IsNullOrWhiteSpace(personName))
            throw new ArgumentException("Person name cannot be empty", nameof(personName));

        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time", nameof(endTime));

        PersonName = personName.Trim();
        Subject = subject?.Trim() ?? string.Empty;
        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Compares events by start time for sorting
    /// </summary>
    public int CompareTo(CalendarEvent? other)
    {
        if (other == null) return 1;
        return StartTime.CompareTo(other.StartTime);
    }

    /// <summary>
    /// Returns a string representation of the event
    /// </summary>
    public override string ToString()
    {
        return $"{PersonName}: {Subject} ({StartTime:HH:mm} - {EndTime:HH:mm})";
    }
}
