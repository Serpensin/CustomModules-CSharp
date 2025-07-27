# SerpentModding

SerpentModding is a modular .NET 9 Windows Forms library designed to provide advanced UI management, animated transitions, and robust logging for desktop applications. It is intended for developers who want to build modern, extensible, and visually appealing WinForms applications with minimal boilerplate.

## Features

- **UIController**: Static class for managing the main window's title, icon, and animated transitions between registered `UserControl` views. Supports multiple transition directions and easing modes for smooth UI animations.
- **KITTScanner**: A reusable component that displays a KITT-style scanning beam animation on any target `Control`. Fully customizable in color, speed, and beam width.
- **Logger**: A singleton logger with support for log levels, file and console output, and automatic exception handling. Designed for easy integration and robust diagnostics.

## Use Cases
- Rapidly build modular WinForms applications with animated UI transitions.
- Add visually engaging scanning effects to controls (e.g., dashboards, status panels).
- Integrate structured logging and error tracking into your application.

## Quick Start

### 1. UIController Example
```csharp
// Initialize in your main form
UIController.Init(this);
UIController.RegisterControl("Home", homeUserControl);
UIController.RegisterControl("Settings", settingsUserControl);

// Remove a registered control by name
UIController.RemoveControl("Settings");

// Switch views with animation
UIController.ShowControl("Home", UIController.TransitionDirection.Left, 500, UIController.EasingMode.EaseInOut);

// Set window title and icon
UIController.SetWindowTitle("My App");
UIController.SetWindowIcon(myIcon);
```

### 2. KITTScanner Example
```csharp
// Attach to a Panel or any Control
var scanner = new KITTScanner(myPanel)
{
    BeamWidth = 80,
    Speed = 5,
    MainColor = Color.Lime
};
scanner.Start();
// ... later
scanner.Stop();
```

### 3. Logger Example
```csharp
Logger.Instance.Initialize(LogLevel.Info);
Logger.Instance.Info("Application started");
Logger.Instance.Error("Something went wrong", ex);
```

## API Overview
- **UIController**: `Init`, `RegisterControl`, `ShowControl`, `SetWindowTitle`, `SetTemporaryTitle`, `ResetTitle`, `SetWindowIcon`
- **KITTScanner**: `KITTScanner(Control target)`, `Start()`, `Stop()`, `BeamWidth`, `Speed`, `MainColor`
- **Logger**: `Initialize`, `Log`, `Trace`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`, `ReadAllLogs`, `GetLogFilePath`

## Requirements
- .NET 9 SDK
- Windows OS

## License
See LICENSE for details.
