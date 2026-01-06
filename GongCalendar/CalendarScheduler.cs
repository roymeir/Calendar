namespace GongCalendar;

using GongCalendar.Interfaces;
using GongCalendar.Models;

/// <summary>
/// Main facade for calendar scheduling functionality.
/// This class provides a clean, simple API that matches the required signature
/// while internally using the modular domain models and services.
///
/// Design Pattern: Facade
/// - Simplifies the interface for clients
/// - Hides internal complexity of TimeSlot objects
/// - Provides the exact API signature required by the specification
///
/// This design allows:
/// - Easy mocking for tests
/// - Swapping implementations of IAvailabilityFinder
/// - Adding features like caching, logging, or metrics without changing the API
/// </summary>
public class CalendarScheduler
{
    private readonly IAvailabilityFinder _availabilityFinder;

    /// <summary>
    /// Creates a new calendar scheduler
    /// </summary>
    /// <param name="availabilityFinder">Service for finding available slots (injected)</param>
    public CalendarScheduler(IAvailabilityFinder availabilityFinder)
    {
        _availabilityFinder = availabilityFinder ?? throw new ArgumentNullException(nameof(availabilityFinder));
    }

    /// <summary>
    /// Finds available time slots when all specified people are available.
    /// This is the main API method matching the required specification.
    /// </summary>
    /// <param name="personList">List of person names who must attend</param>
    /// <param name="eventDuration">Duration of the desired meeting</param>
    /// <returns>List of time ranges (Start, End) when meeting can start</returns>
    public List<(TimeOnly Start, TimeOnly End)> FindAvailableSlots(
        List<string> personList,
        TimeSpan eventDuration)
    {
        // Delegate to the availability finder service
        var slots = _availabilityFinder.FindAvailableSlots(personList, eventDuration);

        // Convert TimeSlot objects to tuples for the API
        return slots.Select(s => (s.Start, s.End)).ToList();
    }

}
