# UIController

The `UIController` is a static class for managing user interface transitions and window properties in the CustomModules-CSharp project. It provides methods for:

- Registering and switching between `UserControl` views with animated transitions (slide, bounce, elastic, etc.).
- Setting and resetting the main window title and icon.
- Managing transition directions and easing modes.

## Usage

### 1. Initialization
Call `UIController.Init(Main mainForm)` at application startup.

### 2. Register Controls
Use `UIController.RegisterControl(string name, UserControl control)` to register each view.

### 3. Show Controls
Use `UIController.ShowControl(string name, TransitionDirection direction, int durationMs, EasingMode easingMode)` to switch views with optional animation.

### 4. Window Title and Icon
- **Set Window Title:**
  - `UIController.SetWindowTitle("New Title")` — Temporarily sets the window title.
  - `UIController.SetWindowTitle("Permanent Title", remember: true)` — Sets and remembers the new title as the default.
- **Set Temporary Title:**
  - `UIController.SetTemporaryTitle("(Loading)")` — Appends a suffix to the original title (e.g., "MyApp (Loading)").
- **Reset Title:**
  - `UIController.ResetTitle()` — Restores the original or last-remembered title.
- **Set Window Icon:**
  - `UIController.SetWindowIcon(myIcon)` — Sets the main window's icon. Pass a valid `System.Drawing.Icon` instance.

## API Overview
- `Init(Main main)`
- `RegisterControl(string name, UserControl control)`
- `ShowControl(string name, TransitionDirection, int, EasingMode)`
- `SetWindowTitle(string title, bool remember = false)`
- `SetTemporaryTitle(string suffix)`
- `ResetTitle()`
- `SetWindowIcon(Icon icon)`

## Transition Directions
- Left, Right, Up, Down, None

## Easing Modes
- Linear, EaseIn, EaseOut, EaseInOut, ElasticOut, BounceOut

See the source code for detailed documentation and examples.