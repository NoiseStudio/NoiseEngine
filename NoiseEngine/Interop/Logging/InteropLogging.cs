using System;
using System.Runtime.InteropServices;
using System.Text;
using NoiseEngine.Logging;
using NoiseEngine.Threading;

namespace NoiseEngine.Interop.Logging;

internal static partial class InteropLogging {

    private static AtomicBool initialized;
    private static AtomicBool terminated;

    private static LoggerHandlerDelegate? LoggerHandler { get; set; }

    private static Logger? Logger { get; set; }

    [UnmanagedFunctionPointer(InteropConstants.CallingConvention)]
    private delegate void LoggerHandlerDelegate(LogData logData);

    public static void Initialize(Logger logger) {
        if (initialized.Exchange(true))
            throw new InvalidOperationException("Cannot initialize native logging more than once.");

        Logger = logger;

        // Prevents GC cleanup (https://stackoverflow.com/a/43227979/14677292)
        LoggerHandler = LoggerHandlerImpl;
        InteropInitialize(LoggerHandler);
    }

    public static void Terminate() {
        if (!initialized)
            throw new InvalidOperationException("Cannot terminate native logging before initializing it.");

        if (terminated.Exchange(true))
            throw new InvalidOperationException("Native logging has already been terminated.");

        InteropTerminate();
    }

    [InteropImport("logging_initialize")]
    private static partial void InteropInitialize(LoggerHandlerDelegate handler);

    [InteropImport("logging_terminate")]
    private static partial void InteropTerminate();

    private static void LoggerHandlerImpl(LogData logData) {
        if (Logger is null)
            return;

        NoiseEngine.Logging.LogLevel level = (NoiseEngine.Logging.LogLevel)logData.Level;
        string message = Encoding.UTF8.GetString(logData.Message.AsSpan());
        Logger.Log(level, message);
    }

}
