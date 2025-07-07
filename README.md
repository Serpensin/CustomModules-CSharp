# CustomModules-CSharp

A modular Windows Forms application targeting .NET 9, demonstrating advanced UI transitions and modular design.

**Note:** This solution is a collection of custom modules and does not provide a runnable application out of the box. Building the solution will succeed, but no user-facing functionality is available unless you add your own modules and entry points.

## Features
- Modular UI with animated transitions between views
- Customizable window title and icon
- Extensible via user controls
- Modern C# features (C# 13, .NET 9)

## Structure
- `UIController/` — Contains the UIController static class for managing UI transitions and window properties. See [UIController/README.md](UIController/README.md).
- `KITTScanner/` — Contains the KITTScanner module. See [KITTScanner/README.md](KITTScanner/README.md).
- `Main.cs` — Main application form
- `Program.cs` — Application entry point

## Getting Started
1. **Requirements**: .NET 9 SDK, Windows OS
2. **Build**: `dotnet build`
3. **Run**: `dotnet run` or launch the WinForms app via Visual Studio

## License
See LICENSE (if present) for license information.
