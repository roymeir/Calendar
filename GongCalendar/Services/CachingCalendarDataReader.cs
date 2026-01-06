namespace GongCalendar.Services;

using GongCalendar.Interfaces;
using GongCalendar.Models;

/// <summary>
/// Decorator that adds caching to any ICalendarDataReader implementation.
/// Uses in-memory caching to avoid re-parsing the data source on every request.
///
/// Design Pattern: Decorator
/// - Wraps another ICalendarDataReader (composition)
/// - Adds caching behavior transparently
/// - Follows Open/Closed Principle (extends without modifying)
///
/// When to use:
/// ✅ Interactive applications with multiple searches
/// ✅ Web APIs with frequent queries
/// ✅ Desktop apps with session-based usage
/// ❌ One-time batch processing
/// ❌ Very large datasets that don't fit in memory
/// ❌ Frequently updating data sources
///
/// Performance:
/// - First call: O(n) - reads and caches all events
/// - Subsequent calls: O(1) - returns cached data
/// - Memory: ~40 bytes per event (e.g., 800 KB for 20,000 events)
/// </summary>
public class CachingCalendarDataReader : ICalendarDataReader
{
    private readonly ICalendarDataReader _innerReader;
    private List<CalendarEvent>? _cache = null;
    private readonly object _cacheLock = new object();

    /// <summary>
    /// Creates a caching decorator around another calendar data reader
    /// </summary>
    /// <param name="innerReader">The underlying data reader to cache</param>
    public CachingCalendarDataReader(ICalendarDataReader innerReader)
    {
        _innerReader = innerReader ?? throw new ArgumentNullException(nameof(innerReader));
    }

    /// <summary>
    /// Reads calendar events, using cache if available.
    /// Thread-safe implementation for concurrent access.
    /// </summary>
    /// <returns>Cached or freshly loaded calendar events</returns>
    public IEnumerable<CalendarEvent> ReadCalendarEvents()
    {
        lock (_cacheLock)
        {
            if (_cache == null)
            {
                // First access: load and cache all events
                _cache = _innerReader.ReadCalendarEvents().ToList();
            }
        }

        return _cache;
    }

    public void ClearCache()
    {
        lock (_cacheLock)
        {
            _cache = null;
        }
    }

    public bool IsCached => _cache != null;

    public int CachedEventCount => _cache?.Count ?? 0;
}
