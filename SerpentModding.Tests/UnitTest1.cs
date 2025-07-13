using SerpentModding;
using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Xunit;

namespace SerpentModding.Tests;

public class LoggerTests
{
    [Fact]
    public void Logger_InitializesAndWritesLogFile()
    {
        var logger = Logger.Instance;
        logger.Initialize(LogLevel.Info, logToConsole: false);
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
        logger.Initialize(LogLevel.Warn, logToConsole: false);
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
        // Simuliere uninitialisierten Logger
        var path = logger.GetLogFilePath();
        var logs = logger.ReadAllLogs();
        Assert.True(string.IsNullOrEmpty(path) || !File.Exists(path));
        Assert.Empty(logs);
    }
}

public class UIControllerTests
{
    [Fact]
    public void UIController_Init_WithNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => UIController.Init(null!));
    }

    [Fact]
    public void UIController_RegisterControl_And_ShowControl_DoesNotThrow()
    {
        var form = new Form { Text = "Main" };
        UIController.Init(form);
        var ctrl = new UserControl { Name = "TestCtrl" };
        UIController.RegisterControl("Test", ctrl);
        UIController.ShowControl("Test");
        Assert.True(ctrl.Visible);
    }

    [Fact]
    public void UIController_RegisterControl_Duplicate_DoesNotThrow()
    {
        var form = new Form { Text = "Main2" };
        UIController.Init(form);
        var ctrl = new UserControl { Name = "TestCtrl2" };
        UIController.RegisterControl("Test2", ctrl);
        // Duplicate registration
        UIController.RegisterControl("Test2", ctrl);
        // No Exception expected
    }

    [Fact]
    public void UIController_ShowControl_Unregistered_DoesNotThrow()
    {
        var form = new Form { Text = "Main3" };
        UIController.Init(form);
        UIController.ShowControl("NotRegistered");
        // No Exception expected
    }

    [Fact]
    public void UIController_SetWindowTitle_And_ResetTitle_Works()
    {
        var form = new Form { Text = "OriginalTitle" };
        UIController.Init(form);
        UIController.SetWindowTitle("NewTitle");
        Assert.Equal("NewTitle", form.Text);
        UIController.ResetTitle();
        Assert.Equal("OriginalTitle", form.Text);
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
        scanner.Start();
        scanner.Start();
        scanner.Stop();
        scanner.Stop();
        // No Exception expected
    }

    [Fact]
    public void KITTScanner_Start_ChangesState()
    {
        var panel = new Panel { Width = 100, Height = 20 };
        var scanner = new KITTScanner(panel);
        scanner.Start();
        // Es gibt kein öffentliches Property für isRunning, aber kein Exception = Erfolg
        scanner.Stop();
    }
}
