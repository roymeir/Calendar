namespace GongCalendar.Interfaces;

using GongCalendar.Models;

/// <summary>
/// Service contract for finding available time slots in calendars.
/// This interface allows for multiple implementations with different algorithms
/// or optimization strategies, following the Open/Closed Principle.
/// </summary>
public interface IAvailabilityFinder
{
    /// <summary>
    /// Finds time slots when all specified people are available for a meeting.
    /// Returns time windows representing when a meeting CAN START, not just discrete start times.
    /// </summary>
    /// <param name="personList">List of person names who must attend</param>
    /// <param name="eventDuration">Duration of the desired meeting</param>
    /// <returns>List of available time slots (windows when meeting can start)</returns>
    List<TimeSlot> FindAvailableSlots(List<string> personList, TimeSpan eventDuration);
}
