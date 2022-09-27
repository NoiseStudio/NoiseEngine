using System;
using System.Runtime.InteropServices;
using System.Text;
using NoiseEngine.Logging;

namespace NoiseEngine.Interop.Logging;

internal static partial class InteropLogging {

    private static bool initialized;
    private static bool terminated;

    private static LoggerHandlerDelegate? LoggerHandler { get; set; }

    private static Logger? Logger { get; set; }

    [UnmanagedFunctionPointer(InteropConstants.CallingConvention)]
    private delegate void LoggerHandlerDelegate(LogData logData);

    public static void Initialize(Logger logger) {
        if (initialized)
            throw new InvalidOperationException("Cannot initialize native logging more than once.");

        initialized = true;
        Logger = logger;

        // Prevents GC cleanup (https://stackoverflow.com/a/43227979/14677292)
        LoggerHandler = LoggerHandlerImpl;
        InteropInitialize(LoggerHandler);
    }

    public static void Terminate() {
        if (!initialized)
            throw new InvalidOperationException("Cannot terminate native logging before initializing it.");

        if (terminated)
            throw new InvalidOperationException("Native logging has already been terminated.");

        terminated = true;
        InteropTerminate();
    }

    [InteropImport("logging_initialize")]
    private static partial void InteropInitialize(LoggerHandlerDelegate handler);

    [InteropImport("logging_terminate")]
    private static partial void InteropTerminate();

    private static void LoggerHandlerImpl(LogData logData) {
        if (Logger is null)
            return;

        NoiseEngine.Logging.LogLevel level = logData.Level switch {
            LogLevel.Trace => NoiseEngine.Logging.LogLevel.Trace,
            LogLevel.Debug => NoiseEngine.Logging.LogLevel.Debug,
            LogLevel.Info => NoiseEngine.Logging.LogLevel.Info,
            LogLevel.Warning => NoiseEngine.Logging.LogLevel.Warning,
            LogLevel.Error => NoiseEngine.Logging.LogLevel.Error,

            // This should never happen, exception indicates implementation error
            _ => throw new ArgumentOutOfRangeException(nameof(logData), logData.Level, null)
        };

        string message = Encoding.UTF8.GetString(logData.Message.AsSpan());
        Logger.Log(level, message);
    }

}
