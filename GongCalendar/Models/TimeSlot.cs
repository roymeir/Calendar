namespace GongCalendar.Models;

/// <summary>
/// Represents a time window with start and end times.
/// This is a value object that provides methods for working with time ranges,
/// including overlapping detection and merging logic.
/// </summary>
public class TimeSlot : IComparable<TimeSlot>
{
    /// <summary>
    /// Start time of the slot
    /// </summary>
    public TimeOnly Start { get; init; }

    /// <summary>
    /// End time of the slot
    /// </summary>
    public TimeOnly End { get; init; }

    /// <summary>
    /// Creates a new time slot with validation
    /// </summary>
    /// <param name="start">Start time</param>
    /// <param name="end">End time (must be >= start time)</param>
    /// <exception cref="ArgumentException">Thrown when end time is before start time</exception>
    public TimeSlot(TimeOnly start, TimeOnly end)
    {
        if (end < start)
            throw new ArgumentException("End time must be after or equal to start time");

        Start = start;
        End = end;
    }

    /// <summary>
    /// Gets the duration of this time slot
    /// </summary>
    public TimeSpan Duration => End.ToTimeSpan() - Start.ToTimeSpan();

    /// <summary>
    /// Returns whether this is a valid time slot (End >= Start)
    /// </summary>
    public bool IsValid => End >= Start;

    /// <summary>
    /// Checks if this time slot overlaps with another time slot.
    /// Two slots overlap if they share any time period.
    /// </summary>
    /// <param name="other">The other time slot to check</param>
    /// <returns>True if the slots overlap, false otherwise</returns>
    public bool Overlaps(TimeSlot other)
    {
        if (other == null) return false;
        return Start < other.End && End > other.Start;
    }

    /// <summary>
    /// Checks if this time slot can be merged with another slot.
    /// Slots can be merged if they overlap or are adjacent (touching).
    /// </summary>
    /// <param name="other">The other time slot to check</param>
    /// <returns>True if the slots can be merged, false otherwise</returns>
    public bool CanMergeWith(TimeSlot other)
    {
        if (other == null) return false;
        // Slots can merge if they overlap OR are adjacent (touching)
        return Start <= other.End && End >= other.Start;
    }

    /// <summary>
    /// Merges this time slot with another slot to create a new combined slot.
    /// The new slot spans from the earliest start to the latest end.
    /// </summary>
    /// <param name="other">The other time slot to merge with</param>
    /// <returns>A new TimeSlot representing the merged period</returns>
    /// <exception cref="InvalidOperationException">Thrown when slots cannot be merged</exception>
    public TimeSlot MergeWith(TimeSlot other)
    {
        if (!CanMergeWith(other))
            throw new InvalidOperationException("Cannot merge non-overlapping, non-adjacent slots");

        var newStart = Start < other.Start ? Start : other.Start;
        var newEnd = End > other.End ? End : other.End;
        return new TimeSlot(newStart, newEnd);
    }

    /// <summary>
    /// Compares time slots by start time for sorting
    /// </summary>
    public int CompareTo(TimeSlot? other)
    {
        if (other == null) return 1;
        return Start.CompareTo(other.Start);
    }

    /// <summary>
    /// Returns a string representation of the time slot
    /// </summary>
    public override string ToString() => $"{Start:HH:mm} - {End:HH:mm}";

    /// <summary>
    /// Checks equality based on Start and End times
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is TimeSlot other)
            return Start == other.Start && End == other.End;
        return false;
    }

    /// <summary>
    /// Gets hash code based on Start and End times
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(Start, End);
}
