namespace GongCalendar.Tests;

using Xunit;
using GongCalendar.Models;

/// <summary>
/// Tests for TimeSlot merging logic.
/// Focuses on overlapping detection, adjacent slot handling, and merge operations.
/// </summary>
public class TimeSlotMergingTests
{
    [Fact]
    public void Overlaps_WithPartialOverlap_ReturnsTrue()
    {
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(11, 0));

        var result = slot1.Overlaps(slot2);

        Assert.True(result);
    }

    [Fact]
    public void Overlaps_WithAdjacentSlots_ReturnsFalse()
    {
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(9, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0)); // Touching, not overlapping

        var result = slot1.Overlaps(slot2);

        // This is critical: slot1 ends at 9:00, slot2 starts at 9:00
        // They touch but don't overlap (no shared time period)
        Assert.False(result);
    }

    [Fact]
    public void CanMergeWith_WithAdjacentSlots_ReturnsTrue()
    {
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(9, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0)); // Adjacent/touching

        var result = slot1.CanMergeWith(slot2);

        // This is important for the algorithm: we want to merge 08:00-09:00 and 09:00-10:00 into 08:00-10:00
        Assert.True(result);
    }

    [Fact]
    public void MergeWith_WithOverlappingSlots_ReturnsExpandedSlot()
    {
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(11, 0));

        var merged = slot1.MergeWith(slot2);

        Assert.Equal(new TimeOnly(8, 0), merged.Start);
        Assert.Equal(new TimeOnly(11, 0), merged.End);
    }

    [Fact]
    public void MergeWith_WithContainedSlot_ReturnsOuterSlot()
    {
        var outerSlot = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var innerSlot = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0)); // Contained inside

        var merged = outerSlot.MergeWith(innerSlot);

        Assert.Equal(new TimeOnly(8, 0), merged.Start);
        Assert.Equal(new TimeOnly(12, 0), merged.End);
    }

    [Fact]
    public void MergeWith_WithNonMergeableSlots_ThrowsException()
    {
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(9, 0));
        var slot2 = new TimeSlot(new TimeOnly(10, 0), new TimeOnly(11, 0)); // Gap between them

        var exception = Assert.Throws<InvalidOperationException>(() => slot1.MergeWith(slot2));
        Assert.Contains("Cannot merge", exception.Message);
    }
}
