namespace GongCalendar;

/// <summary>
/// This is the App entry point
/// </summary>
public class Program
{
    private const string Logo = @"                                    //
                      *///.      .////*         ,*.
                     ,///////***////////,   *//////
      ./////*,,,,,. *///////////////////////////////
        ,///////////////////////////////////////////          ,,
         *//////////////////////////////////////////////////////.
          .////////////////////////////////////////////////////.
     *////////////////////////////////////////////////////////,
 ///////////////////**#@@@@@@@@@@%///////*/%@@@@@@@@@#/*/////(@@@@@       @@@@@@       &@@@@@@@@@@@/
  .///////////////*%@@@@@@@@@@@@@@%*////%@@@@@@@@@@@@@@@#*///(@@@@@@#     @@@@@@   ,@@@@@@@@@@@@@@@@
     *////////////&@@@@&/*/////////////@@@@@%*////*/&@@@@%*//#@@@@@@@@/   @@@@@@  .@@@@@@.       ,,.
      .//////////%@@@@&*///((((((((//*&@@@@%*//////*(@@@@@(*/%@@@@@@@@@@# @@@@@/  @@@@@@   ,########
     .///////////&@@@@%*/*/&@@@@@@@(*/@@@@@#*//////*(@@@@@(*/&@@@@ (@@@@@@@@@@@.  @@@@@@   /@@@@@@@@
    *////////////%@@@@@/*//***%@@@@(/*#@@@@@//////*/&@@@@%*//@@@@@  .%@@@@@@@@@.  @@@@@@*      @@@@@
  .///////////////%@@@@@@%(((#@@@@&///*#@@@@@@#((%@@@@@@#*/*(@@@@@    /@@@@@@@@.  .&@@@@@@@@(%@@@@@@
///////////////////*#@@@@@@@@@@@@@%/////*(&@@@@@@@@@@&(*///*#@@@@@       @@@@@@.     (@@@@@@@@@@@@@@
          .////////////*********////////////********////////*****/
           ./////////////////////////////////////////////.
          ./////////////////////////////////////////////.
         *//////////////////////////////////////////////.
        ,/////////*  *////////,/////////////////////////.
       ///////,,      ,/////*    *////////*        ,,,*/.
      *///**.           */.        ,////*.
    ,//*                              *,                                                            ";

    public static void Main(string[] args)
    {
        Console.WriteLine(Logo);
        Console.WriteLine("\n\n=== Gong Calendar Scheduler ===\n");

        try
        {
            // Test CSV parsing
            var calendarFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "calendar.csv");
            Console.WriteLine($"Loading calendar from: {calendarFilePath}\n");

            var dataReader = new Services.CsvCalendarDataReader(calendarFilePath);
            var events = dataReader.ReadCalendarEvents().ToList();

            Console.WriteLine($"Successfully loaded {events.Count} calendar events:\n");

            foreach (var evt in events.OrderBy(e => e.PersonName).ThenBy(e => e.StartTime))
            {
                Console.WriteLine($"  {evt}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
