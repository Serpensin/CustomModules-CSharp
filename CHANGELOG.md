## [1.1.1] - 2025-07-30
- Logger: Einheitliche Padding-Formatierung f�r das LogLevel-Feld im Log eingef�hrt, sodass alle Level (z.B. INFO, FATAL) gleich breit erscheinen.
- Logger: Unittest erg�nzt, der pr�ft, dass das LogLevel im Log immer auf 5 Zeichen gepaddet ist.

## [1.1.0] - 2025-07-28
- Added `UIController.RemoveControl(string name)` to allow removing registered UserControls at runtime.
- Documentation: Updated README with RemoveControl usage example in the UIController section.

## [1.0.0] - 2025-07-12
- Initial release for .NET 9. Includes UIController, KITTScanner, and Logger modules.
