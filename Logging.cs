using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SerpentModding
{
    /// <summary>
    /// Represents the severity level of a log message.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Trace level for very detailed logs.</summary>
        Trace,
        /// <summary>Debug level for debugging information.</summary>
        Debug,
        /// <summary>Info level for informational messages.</summary>
        Info,
        /// <summary>Warn level for warning messages.</summary>
        Warn,
        /// <summary>Error level for error messages.</summary>
        Error,
        /// <summary>Fatal level for critical errors.</summary>
        Fatal
    }

    /// <summary>
    /// Provides logging functionality with support for file and console output, log levels, and exception handling.
    /// </summary>
    public sealed class Logger
    {
        private static readonly Lock _lock = new();
        private static Logger? _instance;

        /// <summary>
        /// Gets the singleton instance of the <see cref="Logger"/> class.
        /// </summary>
        public static Logger Instance => _instance ??= new Logger();
        /// <summary>
        /// Gets the full path to the current log file if the logger is initialized; otherwise, returns an empty string.
        /// </summary>
        /// <returns>
        /// The full path to the log file if initialized; otherwise, an empty string.
        /// </returns>
        public string GetLogFilePath()
        {
            return _isInitialized ? _logFilePath : string.Empty;
        }
        /// <summary>
        /// Reads all log entries from the current log file if the logger is initialized and the file exists.
        /// </summary>
        /// <returns>
        /// An array of log entries, or an empty array if the logger is not initialized, the file does not exist, or an error occurs.
        /// </returns>
        public string[] ReadAllLogs()
        {
            try
            {
                return _isInitialized && File.Exists(_logFilePath)
                    ? File.ReadAllLines(_logFilePath)
                    : [];
            }
            catch
            {
                return [];
            }
        }

        private string _logDirectory = "";
        private string _logFilePath = "";
        private bool _logToConsole = true;
        private LogLevel _minimumLevel = LogLevel.Debug;
        private bool _isInitialized = false;

        /// <summary>
        /// Prevents a default instance of the <see cref="Logger"/> class from being created.
        /// </summary>
        private Logger() { }

        /// <summary>
        /// Initializes the logger with the specified minimum log level and console output option.
        /// </summary>
        /// <param name="minimumLevel">The minimum <see cref="LogLevel"/> to log.</param>
        /// <param name="logToConsole">Whether to also log messages to the console.</param>
        public void Initialize(LogLevel minimumLevel = LogLevel.Debug, bool logToConsole = true)
        {
            if (_isInitialized)
                return;

            _minimumLevel = minimumLevel;
            _logToConsole = logToConsole;

            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            _logDirectory = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}-Logs");
            Directory.CreateDirectory(_logDirectory);
            _logFilePath = Path.Combine(_logDirectory, $"{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.log");

            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    Fatal("Unhandled domain exception", ex);
                else
                    Fatal("Unhandled domain exception (non-Exception object)");
            };

            TaskScheduler.UnobservedTaskException += (_, e) =>
            {
                Fatal("Unobserved task exception", e.Exception);
                e.SetObserved();
            };

            _isInitialized = true;
            Info("Logger initialized");
        }

        /// <summary>
        /// Logs a message with the specified log level, message, and optional exception and caller information.
        /// </summary>
        /// <param name="level">The <see cref="LogLevel"/> of the log entry.</param>
        /// <param name="message">The log message.</param>
        /// <param name="ex">The exception to log, if any.</param>
        /// <param name="caller">The name of the calling member (automatically supplied).</param>
        /// <param name="file">The file path of the calling member (automatically supplied).</param>
        /// <param name="line">The line number in the source file (automatically supplied).</param>
        public void Log(
            LogLevel level,
            string message,
            Exception? ex = null,
            [CallerMemberName] string? caller = null,
            [CallerFilePath] string? file = null,
            [CallerLineNumber] int line = 0)
        {
            if (!_isInitialized || level < _minimumLevel)
                return;

            if (ex != null)
            {
                LogException(level, message, ex);
                return;
            }

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var callerNamespace = GetCallingNamespace();
            var className = Path.GetFileNameWithoutExtension(file ?? "UnknownClass");
            var location = $"{className}.{caller}():{line}";
            var formatted = $"[{timestamp}] [{level.ToString().ToUpper()}] {{{callerNamespace}}} [{location}] {message}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, formatted + Environment.NewLine);
                if (_logToConsole)
                {
                    var previousColor = Console.ForegroundColor;
                    Console.ForegroundColor = GetColor(level);
                    Console.WriteLine(formatted);
                    Console.ForegroundColor = previousColor;
                }
            }
        }

        /// <summary>
        /// Logs a trace-level message.
        /// </summary>
        /// <param name="msg">The log message.</param>
        /// <param name="ex">The exception to log, if any.</param>
        /// <param name="c">The name of the calling member (automatically supplied).</param>
        /// <param name="f">The file path of the calling member (automatically supplied).</param>
        /// <param name="l">The line number in the source file (automatically supplied).</param>
        public void Trace(string msg, Exception? ex = null, [CallerMemberName] string? c = null, [CallerFilePath] string? f = null, [CallerLineNumber] int l = 0)
            => Log(LogLevel.Trace, msg, ex, c, f, l);

        /// <summary>
        /// Logs a debug-level message.
        /// </summary>
        /// <param name="msg">The log message.</param>
        /// <param name="ex">The exception to log, if any.</param>
        /// <param name="c">The name of the calling member (automatically supplied).</param>
        /// <param name="f">The file path of the calling member (automatically supplied).</param>
        /// <param name="l">The line number in the source file (automatically supplied).</param>
        public void Debug(string msg, Exception? ex = null, [CallerMemberName] string? c = null, [CallerFilePath] string? f = null, [CallerLineNumber] int l = 0)
            => Log(LogLevel.Debug, msg, ex, c, f, l);

        /// <summary>
        /// Logs an info-level message.
        /// </summary>
        /// <param name="msg">The log message.</param>
        /// <param name="ex">The exception to log, if any.</param>
        /// <param name="c">The name of the calling member (automatically supplied).</param>
        /// <param name="f">The file path of the calling member (automatically supplied).</param>
        /// <param name="l">The line number in the source file (automatically supplied).</param>
        public void Info(string msg, Exception? ex = null, [CallerMemberName] string? c = null, [CallerFilePath] string? f = null, [CallerLineNumber] int l = 0)
            => Log(LogLevel.Info, msg, ex, c, f, l);

        /// <summary>
        /// Logs a warning-level message.
        /// </summary>
        /// <param name="msg">The log message.</param>
        /// <param name="ex">The exception to log, if any.</param>
        /// <param name="c">The name of the calling member (automatically supplied).</param>
        /// <param name="f">The file path of the calling member (automatically supplied).</param>
        /// <param name="l">The line number in the source file (automatically supplied).</param>
        public void Warn(string msg, Exception? ex = null, [CallerMemberName] string? c = null, [CallerFilePath] string? f = null, [CallerLineNumber] int l = 0)
            => Log(LogLevel.Warn, msg, ex, c, f, l);

        /// <summary>
        /// Logs an error-level message.
        /// </summary>
        /// <param name="msg">The log message.</param>
        /// <param name="ex">The exception to log, if any.</param>
        /// <param name="c">The name of the calling member (automatically supplied).</param>
        /// <param name="f">The file path of the calling member (automatically supplied).</param>
        /// <param name="l">The line number in the source file (automatically supplied).</param>
        public void Error(string msg, Exception? ex = null, [CallerMemberName] string? c = null, [CallerFilePath] string? f = null, [CallerLineNumber] int l = 0)
            => Log(LogLevel.Error, msg, ex, c, f, l);

        /// <summary>
        /// Logs a fatal-level message.
        /// </summary>
        /// <param name="msg">The log message.</param>
        /// <param name="ex">The exception to log, if any.</param>
        /// <param name="c">The name of the calling member (automatically supplied).</param>
        /// <param name="f">The file path of the calling member (automatically supplied).</param>
        /// <param name="l">The line number in the source file (automatically supplied).</param>
        public void Fatal(string msg, Exception? ex = null, [CallerMemberName] string? c = null, [CallerFilePath] string? f = null, [CallerLineNumber] int l = 0)
            => Log(LogLevel.Fatal, msg, ex, c, f, l);


        /// <summary>
        /// Gets the namespace of the calling method from the current stack trace.
        /// </summary>
        /// <returns>The namespace of the calling method, or "UnknownNamespace" if not found.</returns>
        private static string GetCallingNamespace()
        {
            var stackTrace = new StackTrace();
            foreach (var frame in stackTrace.GetFrames()!)
            {
                var method = frame.GetMethod();
                var declaringType = method?.DeclaringType;
                if (declaringType == null) continue;
                var ns = declaringType.Namespace;
                if (!string.IsNullOrEmpty(ns) && ns != typeof(Logger).Namespace)
                    return ns;
            }
            return "UnknownNamespace";
        }

        /// <summary>
        /// Gets the namespace of the calling method from the stack trace of an exception.
        /// </summary>
        /// <param name="ex">The exception to analyze.</param>
        /// <returns>The namespace of the calling method, or "UnknownNamespace" if not found.</returns>
        private static string GetCallingNamespaceFromStack(Exception ex)
        {
            var trace = new StackTrace(ex, true);
            foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
            {
                var ns = frame.GetMethod()?.DeclaringType?.Namespace;
                if (!string.IsNullOrEmpty(ns) && ns != typeof(Logger).Namespace)
                    return ns;
            }
            return "UnknownNamespace";
        }

        /// <summary>
        /// Gets the location (class, method, and line number) from the stack trace of an exception.
        /// </summary>
        /// <param name="ex">The exception to analyze.</param>
        /// <returns>A string representing the location, or "UnknownLocation" if not found.</returns>
        private static string GetLocationFromException(Exception ex)
        {
            var trace = new StackTrace(ex, true);
            foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
            {
                var method = frame.GetMethod();
                if (method == null) continue;
                var cls = method.DeclaringType?.Name ?? "UnknownClass";
                var name = method.Name;
                var line = frame.GetFileLineNumber();
                return $"{cls}.{name}():{line}";
            }
            return "UnknownLocation";
        }

        /// <summary>
        /// Gets the console color associated with a log level.
        /// </summary>
        /// <param name="level">The <see cref="LogLevel"/>.</param>
        /// <returns>The <see cref="ConsoleColor"/> for the log level.</returns>
        private static ConsoleColor GetColor(LogLevel level) => level switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warn => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.Magenta,
            _ => ConsoleColor.White
        };

        /// <summary>
        /// Logs an exception with the specified log level and message.
        /// </summary>
        /// <param name="level">The <see cref="LogLevel"/> of the log entry.</param>
        /// <param name="message">The log message.</param>
        /// <param name="ex">The exception to log.</param>
        private void LogException(LogLevel level, string message, Exception ex)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var callerNamespace = GetCallingNamespaceFromStack(ex);
            var location = GetLocationFromException(ex);
            var formatted = $"[{timestamp}] [{level.ToString().ToUpper()}] {{{callerNamespace}}} [{location}] {message}" +
                            $"{Environment.NewLine}>> Exception: {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}";

            lock (_lock)
            {
                File.AppendAllText(_logFilePath, formatted + Environment.NewLine);
                if (_logToConsole)
                {
                    var previousColor = Console.ForegroundColor;
                    Console.ForegroundColor = GetColor(level);
                    Console.WriteLine(formatted);
                    Console.ForegroundColor = previousColor;
                }
            }
        }
    }
}