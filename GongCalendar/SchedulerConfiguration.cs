namespace GongCalendar;

/// <summary>
/// Configuration settings for the calendar scheduler.
/// Centralizes all configurable parameters to avoid hard-coded values.
/// </summary>
public class SchedulerConfiguration
{
    /// <summary>
    /// Path to the calendar CSV file.
    /// Default: "Resources/calendar.csv" in application directory
    /// </summary>
    public string CalendarFilePath { get; set; }

    /// <summary>
    /// Start of working hours.
    /// Default: 07:00
    /// </summary>
    public TimeOnly WorkingHoursStart { get; set; }

    /// <summary>
    /// End of working hours.
    /// Default: 19:00
    /// </summary>
    public TimeOnly WorkingHoursEnd { get; set; }

    /// <summary>
    /// Whether to enable caching for better performance with multiple queries.
    /// Default: false (streaming mode for minimal memory usage)
    /// </summary>
    public bool EnableCaching { get; set; }

    /// <summary>
    /// Creates a configuration with default values.
    /// </summary>
    public SchedulerConfiguration()
    {
        CalendarFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "calendar.csv");
        WorkingHoursStart = new TimeOnly(7, 0);
        WorkingHoursEnd = new TimeOnly(19, 0);
        EnableCaching = true;
    }

    /// <summary>
    /// Creates a configuration with custom values.
    /// </summary>
    public SchedulerConfiguration(
        string calendarFilePath,
        TimeOnly workingHoursStart,
        TimeOnly workingHoursEnd,
        bool enableCaching = false)
    {
        if (string.IsNullOrWhiteSpace(calendarFilePath))
            throw new ArgumentException("Calendar file path cannot be empty", nameof(calendarFilePath));

        if (workingHoursEnd <= workingHoursStart)
            throw new ArgumentException("Working hours end must be after start", nameof(workingHoursEnd));

        CalendarFilePath = calendarFilePath;
        WorkingHoursStart = workingHoursStart;
        WorkingHoursEnd = workingHoursEnd;
        EnableCaching = enableCaching;
    }

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(CalendarFilePath))
            throw new InvalidOperationException("Calendar file path is not configured");

        if (!File.Exists(CalendarFilePath))
            throw new FileNotFoundException($"Calendar file not found: {CalendarFilePath}");

        if (WorkingHoursEnd <= WorkingHoursStart)
            throw new InvalidOperationException("Working hours end must be after start");
    }
}
