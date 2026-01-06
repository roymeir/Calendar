# Gong Calendar Scheduler

A calendar scheduling application that finds available time slots when all requested attendees are free.

## Purpose

This application solves a common scheduling problem: **finding meeting times when multiple people are available**. Given a list of attendees and a meeting duration, it analyzes their calendars and returns all time windows where the meeting can be scheduled.

**Core Feature:**
```csharp
// Find 60-minute slots when Alice and Jack are both free
var slots = scheduler.FindAvailableSlots(new List<string> { "Alice", "Jack" }, TimeSpan.FromMinutes(60));
// Returns: [(07:00, 07:00), (09:40, 12:00), (14:00, 15:00), (17:00, 18:00)]
```

**Key Constraints:**
- Single-day calendar (times only, no dates)
- Working hours: 07:00 to 19:00
- Returns time windows representing when a meeting **can start** (not just free time)

---

## Architecture

### Design Principles

The solution follows **SOLID principles** with a layered, modular architecture:

- **Single Responsibility**: Each class has one job (parsing, finding availability, caching)
- **Open/Closed**: New data sources can be added without modifying existing code
- **Dependency Inversion**: High-level modules depend on abstractions (interfaces)
- **Decorator Pattern**: Caching wraps any data reader transparently

### Project Structure

```
GongCalendar/
├── Program.cs                  # Console app entry point with examples
├── CalendarScheduler.cs        # Public API facade (main entry point for consumers)
├── SchedulerConfiguration.cs   # Centralized configuration settings
│
├── Interfaces/
│   ├── ICalendarDataReader.cs  # Abstraction for data sources
│   └── IAvailabilityFinder.cs  # Abstraction for availability algorithm
│
├── Models/
│   ├── CalendarEvent.cs        # Domain model: calendar event
│   └── TimeSlot.cs             # Domain model: time range with merge logic
│
├── Services/
│   ├── CsvCalendarDataReader.cs      # CSV parsing (streaming)
│   ├── CachingCalendarDataReader.cs  # Caching decorator
│   └── AvailabilityFinderService.cs  # Core scheduling algorithm
│
└── Resources/
    └── calendar.csv            # Sample calendar data

GongCalendar.Tests/
├── AvailabilityFinderTests.cs  # Core algorithm tests
├── CsvParsingTests.cs          # CSV parsing edge cases
├── TimeSlotMergingTests.cs     # TimeSlot model tests
├── CachingTests.cs             # Caching behavior tests
└── TestData/                   # Test CSV files
```

### Component Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        CalendarScheduler                         │
│                     (Public API Facade)                          │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    IAvailabilityFinder                           │
│              (AvailabilityFinderService)                         │
│                                                                  │
│  Algorithm: Merge-and-Invert                                     │
│  1. Collect busy periods for all attendees                       │
│  2. Merge overlapping/adjacent busy slots                        │
│  3. Invert to find free gaps                                     │
│  4. Adjust for meeting duration                                  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                    ICalendarDataReader                           │
│                                                                  │
│  ┌──────────────────────┐    ┌─────────────────────────────┐    │
│  │ CsvCalendarDataReader│◄───│ CachingCalendarDataReader   │    │
│  │    (Streaming)       │    │       (Decorator)           │    │
│  └──────────────────────┘    └─────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
```

---

## Entry Points

### 1. Console Application (Demo)

```bash
dotnet run --project GongCalendar
```

Runs `Program.cs` which demonstrates three example queries with formatted output.

### 2. Programmatic API

```csharp
// Setup with dependency injection
var dataReader = new CsvCalendarDataReader("calendar.csv");
var availabilityFinder = new AvailabilityFinderService(dataReader);
var scheduler = new CalendarScheduler(availabilityFinder);

// Find available slots
var slots = scheduler.FindAvailableSlots(
    new List<string> { "Alice", "Jack" },
    TimeSpan.FromMinutes(60)
);

// Process results
foreach (var (start, end) in slots)
{
    Console.WriteLine($"Meeting can start between: {start:HH:mm} - {end:HH:mm}");
}
```

### 3. With Configuration

```csharp
var config = new SchedulerConfiguration
{
    CalendarFilePath = "path/to/calendar.csv",
    WorkingHoursStart = new TimeOnly(9, 0),   // Custom: 9 AM
    WorkingHoursEnd = new TimeOnly(17, 0),    // Custom: 5 PM
    EnableCaching = true                       // Enable for multiple queries
};

config.Validate();

var dataReader = new CsvCalendarDataReader(config.CalendarFilePath);
ICalendarDataReader reader = config.EnableCaching
    ? new CachingCalendarDataReader(dataReader)
    : dataReader;

var finder = new AvailabilityFinderService(
    reader,
    config.WorkingHoursStart,
    config.WorkingHoursEnd
);
var scheduler = new CalendarScheduler(finder);
```

---

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test -v detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~AvailabilityFinderTests"
```

### Test Coverage

| Test Class | Focus Area |
|------------|------------|
| `AvailabilityFinderTests` | Core algorithm: merging, clipping, duration adjustment, edge cases |
| `CsvParsingTests` | CSV parsing: quoted fields, malformed data, streaming behavior |
| `TimeSlotMergingTests` | TimeSlot model: overlap detection, merge operations |
| `CachingTests` | Caching decorator: cache behavior, thread safety |

### Key Test Scenarios

- **Overlapping events** - Events that share time are merged correctly
- **Working hours clipping** - Events outside 07:00-19:00 are clipped
- **Multiple attendees** - Union of all busy periods
- **Zero-length slots** - Exactly-fitting meetings (e.g., `07:00-07:00`)
- **Case-insensitive matching** - "Alice", "alice", "ALICE" all match
- **Unknown persons** - Treated as free all day
- **Malformed CSV** - Invalid lines skipped, valid lines processed

---

## Optimizations for Scale and Speed

### 1. Streaming CSV Parser

```csharp
// Uses File.ReadLines() + yield return for lazy evaluation
public IEnumerable<CalendarEvent> ReadCalendarEvents()
{
    foreach (var line in File.ReadLines(_filePath))  // Streams line-by-line
    {
        yield return ParseLine(line);  // Lazy evaluation
    }
}
```

**Benefits:**
- **O(1) memory** regardless of file size
- 20,000 events: ~12 KB memory vs ~800 KB with `ReadAllLines()`
- Processing starts immediately, no upfront load

### 2. Optional Caching Layer (Decorator Pattern)

```csharp
// Wrap any reader with caching for repeated queries
ICalendarDataReader reader = new CachingCalendarDataReader(csvReader);
```

**Benefits:**
- First query: O(n) - reads and caches
- Subsequent queries: O(1) - instant cache hit
- Thread-safe with lock-based synchronization
- ~40 bytes per event (~800 KB for 20,000 events)

### 3. Efficient Algorithm - O(n log n)

The merge-and-invert algorithm is optimized for performance:

```
Step 1: Collect busy slots     O(n)   - Single pass through events
Step 2: Sort by start time     O(n log n) - Standard sort
Step 3: Merge overlapping      O(n)   - Single pass merge
Step 4: Invert to free slots   O(n)   - Single pass inversion
Step 5: Adjust for duration    O(n)   - Single pass filter

Total: O(n log n) where n = number of calendar events
```

### 4. HashSet for Person Lookup

```csharp
// O(1) person name lookup instead of O(m) list search
var peopleSet = new HashSet<string>(personList, StringComparer.OrdinalIgnoreCase);
```

**Benefits:**
- Searching for 5 people in 20,000 events: O(n) instead of O(n*m)
- Case-insensitive comparison built-in

### 5. Memory-Efficient Data Structures

- `TimeOnly` instead of `DateTime` (smaller footprint)
- `init` properties on models (immutable, safe)
- No intermediate `ToList()` calls in LINQ chains where possible

---

## Build & Run Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Build Release
dotnet build -c Release

# Run application
dotnet run --project GongCalendar

# Run tests
dotnet test
```

---

## Example Output

For the sample `calendar.csv` with Alice and Jack, requesting a 60-minute meeting:

```
Example 1: Alice & Jack - 60 minute meeting
=------------------------------------------------
Finding available slots for: Alice, Jack
Meeting duration: 60 minutes

Available time slots:
  Meeting can start between: 07:00 - 07:00 (0 minutes window)
  Meeting can start between: 09:40 - 12:00 (140 minutes window)
  Meeting can start between: 14:00 - 15:00 (60 minutes window)
  Meeting can start between: 17:00 - 18:00 (60 minutes window)
```

---

## Design Decisions

| Decision | Rationale |
|----------|-----------|
| Tuples in public API | Matches spec signature; `TimeSlot` used internally for logic |
| Zero-length slots included | Matches expected output in MAIN_README (e.g., `07:00-07:00`) |
| Streaming by default | Optimizes for one-shot queries; caching opt-in for repeated use |
| Case-insensitive names | More user-friendly; avoids "Alice" vs "alice" mismatches |
| Graceful CSV error handling | Skips bad lines, continues processing valid data |
