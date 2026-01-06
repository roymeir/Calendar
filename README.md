# Gong In-Office Coding Evaluation - C#

Welcome to the C# starter project for Gong's coding evaluation!

## Getting Started

### Prerequisites

You will need the following installed on your machine:
- .NET 6.0 SDK or higher
- An IDE (Visual Studio, Visual Studio Code, or JetBrains Rider recommended)

To verify your .NET installation:
```bash
dotnet --version
```

### Setup

1. Restore dependencies:
```bash
dotnet restore
```

This will download all required NuGet packages.

### Running the Application

To run the application:
```bash
dotnet run --project GongCalendar
```

### Building the Project

To build the solution:
```bash
dotnet build
```

To build in Release mode:
```bash
dotnet build -c Release
```

### Running Tests

To run all tests:
```bash
dotnet test
```

To run tests with detailed output:
```bash
dotnet test -v detailed
```

### Opening in Visual Studio

1. Open Visual Studio
2. Select **File → Open → Project/Solution**
3. Navigate to the `csharp-project` directory
4. Select the `GongCalendar.sln` file
5. Click **Open**
6. Visual Studio will automatically restore NuGet packages

Once opened, you can:
- Run the application by pressing **F5** or clicking the **Start** button
- Run tests using the Test Explorer (Test → Test Explorer)
- Set breakpoints and debug your code

### Opening in Visual Studio Code

1. Open Visual Studio Code
2. Select **File → Open Folder...**
3. Navigate to and select the `csharp-project` directory
4. Install the **C# Dev Kit** extension if prompted
5. VS Code will automatically detect the solution and restore packages

Once opened, you can:
- Run the application from the terminal: `dotnet run --project GongCalendar`
- Run tests from the terminal: `dotnet test`
- Use the built-in debugger with launch configurations
- See compiler errors and warnings in real-time

### Opening in JetBrains Rider

1. Open JetBrains Rider
2. Select **File → Open...**
3. Navigate to the `csharp-project` directory
4. Select the `GongCalendar.sln` file
5. Click **OK**
6. Rider will automatically restore NuGet packages and index the solution

Once opened, you can:
- Run the application by clicking the run button or pressing **Shift+F10**
- Run tests using the Unit Tests window
- Use the powerful debugger and profiling tools

## Project Structure

```
csharp-project/
├── GongCalendar/              # Main application project
│   ├── Program.cs             # Application entry point
│   ├── Resources/
│   │   └── calendar.csv       # Example calendar data
│   └── GongCalendar.csproj    # Project configuration
├── GongCalendar.Tests/        # Test project
│   ├── CalendarTests.cs       # Unit tests
│   └── GongCalendar.Tests.csproj  # Test project configuration
├── GongCalendar.sln          # Solution file
└── README.md                 # This file
```

## Your Task

Implement a calendar application that can find available time slots. See the main [README.md](../README.md) in the root directory for complete requirements.

### Method Signature

```csharp
using System;
using System.Collections.Generic;

// Returns a list of time ranges (start, end) representing continuous periods when a meeting can start.
// Note: You may choose to create a custom class instead of using a Tuple if you prefer.
public List<(TimeOnly Start, TimeOnly End)> FindAvailableSlots(List<string> personList, TimeSpan eventDuration);
```

**Parameters:**
- `personList`: List of person names who should attend the meeting
- `eventDuration`: Duration of the desired meeting as a `TimeSpan`

**Returns:**
- List of time ranges (tuples with Start and End times as `TimeOnly` objects) representing continuous periods when a meeting can start

## Tips

- The calendar data is available in `GongCalendar/Resources/calendar.csv`
- Use `System.IO.File` to read the CSV file
- Use `TimeOnly` and `TimeSpan` for time operations
- Consider creating classes to represent Calendar, Event, Person, etc.
- Follow C# naming conventions (PascalCase for public members, camelCase for private)
- Use LINQ for collection operations where appropriate
- Write clean, modular, and well-documented code with XML comments
- Don't forget to implement 2-3 meaningful tests with xUnit!

Good luck!
