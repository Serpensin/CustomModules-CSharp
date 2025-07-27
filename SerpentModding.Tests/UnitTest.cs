using Xunit;


namespace SerpentModding.Tests
{
    [Collection("STA Tests")]
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class LoggerTests : IDisposable
    {
        private readonly string _tempLogDirectory;
        private bool _disposed;

        public LoggerTests()
        {
            _tempLogDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempLogDirectory);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Directory.Delete(_tempLogDirectory, true);
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void Logger_InitializesAndWritesLogFile()
        {
            var logger = Logger.Instance;
            logger.Initialize(LogLevel.Info, logToConsole: false, logDirectory: _tempLogDirectory);
            var logPath = logger.GetLogFilePath();
            logger.Info("Test log entry");
            var logs = logger.ReadAllLogs();
            Assert.True(File.Exists(logPath));
            Assert.Contains(logs, l => l.Contains("Test log entry"));
        }
    
        [Fact]
        public void Logger_LogsErrorWithException()
        {
            var logger = Logger.Instance;
            logger.Initialize(LogLevel.Info, logToConsole: false);
            var ex = new InvalidOperationException("Test exception");
            logger.Error("Error occurred", ex);
            var logs = logger.ReadAllLogs();
            Assert.Contains(logs, l => l.Contains("Error occurred"));
            Assert.Contains(logs, l => l.Contains("Test exception"));
        }
    
        [Fact]
        public void Logger_LogLevelFiltering_Works()
        {
            var logger = Logger.Instance;
            // Use a unique log directory for this test to avoid log file contamination
            var uniqueLogDir = Path.Combine(_tempLogDirectory, Guid.NewGuid().ToString());
            Directory.CreateDirectory(uniqueLogDir);
            logger.Initialize(LogLevel.Warn, logToConsole: false, logDirectory: uniqueLogDir);
            logger.Info("ShouldNotAppear");
            logger.Warn("ShouldAppear");
            var logs = logger.ReadAllLogs();
            Assert.DoesNotContain(logs, l => l.Contains("ShouldNotAppear"));
            Assert.Contains(logs, l => l.Contains("ShouldAppear"));
        }
    
        [Fact]
        public void Logger_MultipleInitializations_DoesNotThrow()
        {
            var logger = Logger.Instance;
            logger.Initialize(LogLevel.Info, logToConsole: false);
            logger.Initialize(LogLevel.Debug, logToConsole: true);
            logger.Info("MultipleInit");
            var logs = logger.ReadAllLogs();
            Assert.Contains(logs, l => l.Contains("MultipleInit"));
        }
    
        [Fact]
        public void Logger_GetLogFilePath_And_ReadAllLogs_BeforeInit()
        {
            var logger = Logger.Instance;
            // Simulate uninitialized Logger
            var path = logger.GetLogFilePath();
            var logs = logger.ReadAllLogs();
            Assert.True(string.IsNullOrEmpty(path) || !File.Exists(path));
            Assert.Empty(logs);
        }
    }
    
    public class UIControllerTests : IDisposable
    {
        private bool _disposed;
        private Form? _form;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _form?.Dispose();
                    _form = null;
                    UIController.Test_Reset();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        [Fact]
        public void UIController_Init_WithNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => UIController.Init(null!));
        }
    
        [Fact]
        public void UIController_RegisterControl_And_ShowControl_DoesNotThrow()
        {
            _form = new Form { Text = "Main" };
            UIController.Init(_form);
            var ctrl = new UserControl { Name = "TestCtrl" };
            UIController.RegisterControl("Test", ctrl);
            UIController.ShowControl("Test");
            Assert.True(ctrl.Visible);
        }
    
        [Fact]
        public void UIController_RegisterControl_DuplicateName_ThrowsArgumentException()
        {
            _form = new Form { Text = "Main2" };
            UIController.Init(_form);
            var ctrl1 = new UserControl { Name = "TestCtrl2" };
            var ctrl2 = new UserControl { Name = "TestCtrl3" };
            UIController.RegisterControl("Test2", ctrl1);
            Assert.Throws<ArgumentException>(() => UIController.RegisterControl("Test2", ctrl2));
        }

        [Fact]
        public void UIController_RegisterControl_SameControlDifferentName_ThrowsInvalidOperationException()
        {
            _form = new Form { Text = "Main2" };
            UIController.Init(_form);
            var ctrl = new UserControl { Name = "TestCtrl2" };
            UIController.RegisterControl("Test2", ctrl);
            Assert.Throws<InvalidOperationException>(() => UIController.RegisterControl("OtherName", ctrl));
        }
    
        [Fact]
        public void UIController_ShowControl_Unregistered_ThrowsKeyNotFoundException()
        {
            _form = new Form { Text = "Main3" };
            UIController.Init(_form);
            Assert.Throws<KeyNotFoundException>(() => UIController.ShowControl("NotRegistered"));
        }
    
        [Fact]
        public void UIController_SetWindowTitle_And_ResetTitle_Works()
        {
            _form = new Form { Text = "OriginalTitle" };
            UIController.Init(_form);
            UIController.SetWindowTitle("NewTitle");
            Assert.Equal("NewTitle", _form.Text);
            UIController.ResetTitle();
            Assert.Equal("OriginalTitle", _form.Text);
        }

        [Fact]
        public void UIController_ShowControl_NotInitialized_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => UIController.ShowControl("AnyName"));
        }

        [Fact]
        public void UIController_RemoveControl_RemovesSuccessfully()
        {
            _form = new Form { Text = "MainRemove" };
            UIController.Init(_form);
            var ctrl = new UserControl { Name = "RemovableCtrl" };
            UIController.RegisterControl("Removable", ctrl);
            UIController.RemoveControl("Removable");
            Assert.Throws<KeyNotFoundException>(() => UIController.ShowControl("Removable"));
        }

        [Fact]
        public void UIController_RemoveControl_Unregistered_ThrowsKeyNotFoundException()
        {
            _form = new Form { Text = "MainRemove2" };
            UIController.Init(_form);
            Assert.Throws<KeyNotFoundException>(() => UIController.RemoveControl("NotRegistered"));
        }
    }

    public class KITTScannerTests
    {
        [Fact]
        public void KITTScanner_Ctor_WithNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new KITTScanner(null!));
        }

        [Fact]
        public void KITTScanner_StartStop_MultipleTimes_DoesNotThrow()
        {
            var panel = new Panel { Width = 100, Height = 20 };
            var scanner = new KITTScanner(panel);
            Assert.False(scanner.IsRunning);
            scanner.Start();
            Assert.True(scanner.IsRunning);
            scanner.Start();
            Assert.True(scanner.IsRunning);
            scanner.Stop();
            Assert.False(scanner.IsRunning);
            scanner.Stop();
            Assert.False(scanner.IsRunning);
        }

        [Fact]
        public void KITTScanner_Start_ChangesState()
        {
            var panel = new Panel { Width = 100, Height = 20 };
            var scanner = new KITTScanner(panel);
            Assert.False(scanner.IsRunning);
            scanner.Start();
            Assert.True(scanner.IsRunning);
            scanner.Stop();
            Assert.False(scanner.IsRunning);
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member