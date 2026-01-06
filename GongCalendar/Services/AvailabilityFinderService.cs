namespace GongCalendar.Services;

using GongCalendar.Interfaces;
using GongCalendar.Models;

/// <summary>
/// Finds available time slots using a "merge-and-invert" algorithm.
///
/// Algorithm Overview:
/// 1. Collect all busy periods for specified people
/// 2. Merge overlapping/adjacent busy periods
/// 3. Invert busy periods to find free gaps
/// 4. Adjust free gaps based on meeting duration and filter zero-length slots
///
/// Complexity: O(n log n) where n is the number of calendar events
///
/// This implementation is designed for scalability:
/// - Works efficiently with large numbers of events
/// - Configurable working hours
/// - Dependency injection for data source
/// - Extensible for future features (time zones, recurring events, etc.)
/// </summary>
public class AvailabilityFinderService : IAvailabilityFinder
{
    private readonly ICalendarDataReader _dataReader;
    private readonly TimeOnly _workingHoursStart;
    private readonly TimeOnly _workingHoursEnd;

    /// <summary>
    /// Creates a new availability finder service
    /// </summary>
    /// <param name="dataReader">Data source for calendar events (injected dependency)</param>
    /// <param name="workingHoursStart">Start of working hours (default: 07:00)</param>
    /// <param name="workingHoursEnd">End of working hours (default: 19:00)</param>
    public AvailabilityFinderService(
        ICalendarDataReader dataReader,
        TimeOnly? workingHoursStart = null,
        TimeOnly? workingHoursEnd = null)
    {
        _dataReader = dataReader ?? throw new ArgumentNullException(nameof(dataReader));
        _workingHoursStart = workingHoursStart ?? new TimeOnly(7, 0);
        _workingHoursEnd = workingHoursEnd ?? new TimeOnly(19, 0);

        if (_workingHoursEnd <= _workingHoursStart)
            throw new ArgumentException("Working hours end must be after start");
    }

    /// <summary>
    /// Finds available time slots when all specified people are available.
    /// Returns time windows representing when a meeting CAN START.
    /// </summary>
    /// <param name="personList">List of person names (case-insensitive)</param>
    /// <param name="eventDuration">Desired meeting duration</param>
    /// <returns>List of time slots when meeting can start</returns>
    public List<TimeSlot> FindAvailableSlots(List<string> personList, TimeSpan eventDuration)
    {
        if (personList == null || personList.Count == 0)
            return new List<TimeSlot>();

        var workingHoursDuration = _workingHoursEnd.ToTimeSpan() - _workingHoursStart.ToTimeSpan();
        if (eventDuration > workingHoursDuration)
            return new List<TimeSlot>();

        if (eventDuration <= TimeSpan.Zero)
            throw new ArgumentException("Event duration must be positive", nameof(eventDuration));

        var busySlots = GetBusySlotsForPeople(personList);
        var mergedBusySlots = MergeBusySlots(busySlots);
        var freeSlots = InvertToFreeSlots(mergedBusySlots);
        var availableSlots = AdjustForMeetingDuration(freeSlots, eventDuration);

        return availableSlots;
    }

    /// <summary>
    /// Step 1: Collects all busy time slots for the specified people.
    /// Filters events by person name (case-insensitive) and clips to working hours.
    ///
    /// Design note: This method is isolated so it can be extended to:
    /// - Load from different data sources
    /// - Apply person-specific working hours
    /// - Handle time zones
    /// </summary>
    /// <param name="personList">List of person names to collect busy periods for</param>
    /// <returns>List of busy time slots</returns>
    private List<TimeSlot> GetBusySlotsForPeople(List<string> personList)
    {
        var allEvents = _dataReader.ReadCalendarEvents();
        var busySlots = new List<TimeSlot>();

        var peopleSet = new HashSet<string>(
            personList.Select(p => p.Trim()),
            StringComparer.OrdinalIgnoreCase
        );

        var relevantEvents = allEvents.Where(e => peopleSet.Contains(e.PersonName));

        foreach (var evt in relevantEvents)
        {
            var start = evt.StartTime < _workingHoursStart ? _workingHoursStart : evt.StartTime;
            var end = evt.EndTime > _workingHoursEnd ? _workingHoursEnd : evt.EndTime;

            if (end > start)
            {
                busySlots.Add(new TimeSlot(start, end));
            }
        }

        return busySlots;
    }

    /// <summary>
    /// Step 2: Merges overlapping or adjacent time slots.
    ///
    /// Algorithm:
    /// - Sort slots by start time: O(n log n)
    /// - Single pass merge: O(n)
    /// - Total complexity: O(n log n)
    ///
    /// Design note: This method is isolated so the merging logic can be:
    /// - Optimized with different algorithms
    /// - Extended to handle priorities
    /// - Reused for other purposes
    /// </summary>
    /// <param name="busySlots">List of potentially overlapping busy slots</param>
    /// <returns>List of merged, non-overlapping busy slots</returns>
    private List<TimeSlot> MergeBusySlots(List<TimeSlot> busySlots)
    {
        if (busySlots.Count == 0)
            return new List<TimeSlot>();

        var sortedSlots = busySlots.OrderBy(s => s.Start).ToList();
        var merged = new List<TimeSlot> { sortedSlots[0] };

        for (int i = 1; i < sortedSlots.Count; i++)
        {
            var lastMerged = merged[merged.Count - 1];
            var current = sortedSlots[i];

            if (lastMerged.CanMergeWith(current))
            {
                merged[merged.Count - 1] = lastMerged.MergeWith(current);
            }
            else
            {
                merged.Add(current);
            }
        }

        return merged;
    }

    /// <summary>
    /// Step 3: Inverts busy slots to find free periods (gaps).
    ///
    /// Algorithm:
    /// - Start from beginning of working hours
    /// - For each busy slot, create a free slot before it
    /// - Add remaining time after last busy slot
    ///
    /// Design note: This method assumes sorted, non-overlapping busy slots.
    /// It's isolated so it can be extended to:
    /// - Handle multiple working hour ranges
    /// - Apply different gap-finding strategies
    /// - Include breaks or lunch periods
    /// </summary>
    /// <param name="busySlots">List of merged, sorted busy slots</param>
    /// <returns>List of free time slots</returns>
    private List<TimeSlot> InvertToFreeSlots(List<TimeSlot> busySlots)
    {
        var freeSlots = new List<TimeSlot>();
        var currentTime = _workingHoursStart;

        foreach (var busySlot in busySlots.OrderBy(s => s.Start))
        {
            if (currentTime < busySlot.Start)
            {
                freeSlots.Add(new TimeSlot(currentTime, busySlot.Start));
            }

            currentTime = busySlot.End > currentTime ? busySlot.End : currentTime;
        }

        if (currentTime < _workingHoursEnd)
        {
            freeSlots.Add(new TimeSlot(currentTime, _workingHoursEnd));
        }

        return freeSlots;
    }

    /// <summary>
    /// Step 4: Adjusts free slots based on meeting duration.
    /// Returns slots representing when a meeting CAN START (not just any free time).
    /// Includes zero-length slots where there's exactly one possible start time.
    ///
    /// Algorithm:
    /// - For each free slot, calculate "latest start time"
    /// - Latest start = slot.End - eventDuration
    /// - If latest start >= slot.Start, include the window [slot.Start, latest start]
    /// - Otherwise, the gap is too small (meeting doesn't fit)
    ///
    /// Design note: Zero-length slots like [07:00-07:00] indicate a single valid start time.
    /// This can be extended to:
    /// - Filter zero-length slots with a configuration flag
    /// - Apply minimum/maximum meeting duration constraints
    /// - Suggest optimal meeting times within windows
    /// </summary>
    /// <param name="freeSlots">List of free time slots</param>
    /// <param name="eventDuration">Duration of the desired meeting</param>
    /// <returns>List of time windows when meeting can start (including zero-length)</returns>
    private List<TimeSlot> AdjustForMeetingDuration(List<TimeSlot> freeSlots, TimeSpan eventDuration)
    {
        var availableSlots = new List<TimeSlot>();

        foreach (var freeSlot in freeSlots)
        {
            var latestStartTime = freeSlot.End.Add(-eventDuration);

            if (latestStartTime >= freeSlot.Start)
            {
                availableSlots.Add(new TimeSlot(
                    freeSlot.Start,
                    TimeOnly.FromTimeSpan(latestStartTime.ToTimeSpan())
                ));
            }
        }

        return availableSlots;
    }
}
