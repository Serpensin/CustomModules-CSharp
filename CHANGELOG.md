## [1.1.1] - 2025-07-30
- Logger: Added consistent padding formatting for the LogLevel field in logs so that all levels (e.g., INFO, FATAL) appear with equal width.
- Logger: Added a unit test to verify that the LogLevel in logs is always padded to 5 characters.

## [1.1.0] - 2025-07-28
- Added `UIController.RemoveControl(string name)` to allow removing registered UserControls at runtime.
- Documentation: Updated README with RemoveControl usage example in the UIController section.

## [1.0.0] - 2025-07-12
- Initial release for .NET 9. Includes UIController, KITTScanner, and Logger modules.
