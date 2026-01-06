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
        // Arrange
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(11, 0));

        // Act
        var result = slot1.Overlaps(slot2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Overlaps_WithAdjacentSlots_ReturnsFalse()
    {
        // Arrange
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(9, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0)); // Touching, not overlapping

        // Act
        var result = slot1.Overlaps(slot2);

        // Assert - Adjacent slots (touching) should NOT overlap
        // This is critical: slot1 ends at 9:00, slot2 starts at 9:00
        // They touch but don't overlap (no shared time period)
        Assert.False(result);
    }

    [Fact]
    public void CanMergeWith_WithAdjacentSlots_ReturnsTrue()
    {
        // Arrange
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(9, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0)); // Adjacent/touching

        // Act
        var result = slot1.CanMergeWith(slot2);

        // Assert - Adjacent slots CAN be merged (different from overlapping)
        // This is important for the algorithm: we want to merge 08:00-09:00 and 09:00-10:00 into 08:00-10:00
        Assert.True(result);
    }

    [Fact]
    public void MergeWith_WithOverlappingSlots_ReturnsExpandedSlot()
    {
        // Arrange
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(10, 0));
        var slot2 = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(11, 0));

        // Act
        var merged = slot1.MergeWith(slot2);

        // Assert - Merged slot should span from earliest start to latest end
        Assert.Equal(new TimeOnly(8, 0), merged.Start);
        Assert.Equal(new TimeOnly(11, 0), merged.End);
    }

    [Fact]
    public void MergeWith_WithContainedSlot_ReturnsOuterSlot()
    {
        // Arrange
        var outerSlot = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(12, 0));
        var innerSlot = new TimeSlot(new TimeOnly(9, 0), new TimeOnly(10, 0)); // Contained inside

        // Act
        var merged = outerSlot.MergeWith(innerSlot);

        // Assert - Merged slot should be the outer slot (contains the inner one)
        Assert.Equal(new TimeOnly(8, 0), merged.Start);
        Assert.Equal(new TimeOnly(12, 0), merged.End);
    }

    [Fact]
    public void MergeWith_WithNonMergeableSlots_ThrowsException()
    {
        // Arrange
        var slot1 = new TimeSlot(new TimeOnly(8, 0), new TimeOnly(9, 0));
        var slot2 = new TimeSlot(new TimeOnly(10, 0), new TimeOnly(11, 0)); // Gap between them

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => slot1.MergeWith(slot2));
        Assert.Contains("Cannot merge", exception.Message);
    }
}
