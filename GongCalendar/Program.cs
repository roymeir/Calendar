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
            var config = new SchedulerConfiguration
            {
                CalendarFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "calendar.csv"),
                WorkingHoursStart = new TimeOnly(7, 0),
                WorkingHoursEnd = new TimeOnly(19, 0),
                EnableCaching = false
            };

            config.Validate();

            var baseReader = new Services.CsvCalendarDataReader(config.CalendarFilePath);
            ICalendarDataReader dataReader = config.EnableCaching
                ? new Services.CachingCalendarDataReader(baseReader)
                : baseReader;

            IAvailabilityFinder availabilityFinder = new Services.AvailabilityFinderService(
                dataReader,
                config.WorkingHoursStart,
                config.WorkingHoursEnd
            );
            var scheduler = new CalendarScheduler(availabilityFinder);

            // Example 1: Alice & Jack - 60 minute meeting
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
