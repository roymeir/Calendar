using GongCalendar.Interfaces;

namespace GongCalendar;

/// <summary>
/// This is the App entry point
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Setup - Manual Dependency Injection
            var calendarFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "calendar.csv");

            // Option 1: Streaming without cache (best for single searches, minimal memory)
            ICalendarDataReader dataReader = new Services.CsvCalendarDataReader(calendarFilePath);

            // Option 2: With caching (best for multiple searches, uses ~800KB for 20K events)
            // Uncomment the lines below to enable caching:
            // var baseReader = new Services.CsvCalendarDataReader(calendarFilePath);
            // ICalendarDataReader dataReader = new Services.CachingCalendarDataReader(baseReader);

            IAvailabilityFinder availabilityFinder = new Services.AvailabilityFinderService(dataReader);
            var scheduler = new CalendarScheduler(availabilityFinder);

            // Example 1: Find slots for Alice and Jack for 60-minute meeting
            Console.WriteLine("Example 1: Alice & Jack - 60 minute meeting");
            Console.WriteLine("=" + new string('-', 48));
            var people1 = new List<string> { "Alice", "Jack" };
            var duration1 = TimeSpan.FromMinutes(60);

            Console.WriteLine($"Finding available slots for: {string.Join(", ", people1)}");
            Console.WriteLine($"Meeting duration: {duration1.TotalMinutes} minutes\n");

            var availableSlots1 = scheduler.FindAvailableSlots(people1, duration1);

            if (availableSlots1.Count == 0)
            {
                Console.WriteLine("No available time slots found.\n");
            }
            else
            {
                Console.WriteLine("Available time slots:");
                foreach (var (Start, End) in availableSlots1)
                {
                    var windowDuration = (End.ToTimeSpan() - Start.ToTimeSpan()).TotalMinutes;
                    Console.WriteLine($"  Meeting can start between: {Start:HH:mm} - {End:HH:mm} ({windowDuration} minutes window)");
                }
                Console.WriteLine();
            }

            // Example 2: Find slots for Bob for 30-minute meeting
            Console.WriteLine("Example 2: Bob - 30 minute meeting");
            Console.WriteLine("=" + new string('-', 48));
            var people2 = new List<string> { "Bob" };
            var duration2 = TimeSpan.FromMinutes(30);

            Console.WriteLine($"Finding available slots for: {string.Join(", ", people2)}");
            Console.WriteLine($"Meeting duration: {duration2.TotalMinutes} minutes\n");

            var availableSlots2 = scheduler.FindAvailableSlots(people2, duration2);

            if (availableSlots2.Count == 0)
            {
                Console.WriteLine("No available time slots found.\n");
            }
            else
            {
                Console.WriteLine("Available time slots:");
                foreach (var (Start, End) in availableSlots2)
                {
                    var windowDuration = (End.ToTimeSpan() - Start.ToTimeSpan()).TotalMinutes;
                    Console.WriteLine($"  Meeting can start between: {Start:HH:mm} - {End:HH:mm} ({windowDuration} minutes window)");
                }
                Console.WriteLine();
            }

            // Example 3: Find slots for all three people for 120-minute meeting
            Console.WriteLine("Example 3: Alice, Jack & Bob - 120 minute meeting");
            Console.WriteLine("=" + new string('-', 48));
            var people3 = new List<string> { "Alice", "Jack", "Bob" };
            var duration3 = TimeSpan.FromMinutes(120);

            Console.WriteLine($"Finding available slots for: {string.Join(", ", people3)}");
            Console.WriteLine($"Meeting duration: {duration3.TotalMinutes} minutes\n");

            var availableSlots3 = scheduler.FindAvailableSlots(people3, duration3);

            if (availableSlots3.Count == 0)
            {
                Console.WriteLine("No available time slots found.\n");
            }
            else
            {
                Console.WriteLine("Available time slots:");
                foreach (var slot in availableSlots3)
                {
                    var windowDuration = (slot.End.ToTimeSpan() - slot.Start.ToTimeSpan()).TotalMinutes;
                    Console.WriteLine($"  Meeting can start between: {slot.Start:HH:mm} - {slot.End:HH:mm} ({windowDuration} minutes window)");
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
