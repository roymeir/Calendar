namespace GongCalendar.Interfaces;

using GongCalendar.Models;

/// <summary>
/// Contract for reading calendar events from any data source.
/// This interface follows the Dependency Inversion Principle (DIP),
/// allowing high-level modules to depend on this abstraction rather than
/// concrete implementations like CSV readers, database readers, etc.
/// </summary>
public interface ICalendarDataReader
{
    /// <summary>
    /// Reads and returns all calendar events from the data source.
    /// </summary>
    /// <returns>A collection of calendar events</returns>
    IEnumerable<CalendarEvent> ReadCalendarEvents();
}
