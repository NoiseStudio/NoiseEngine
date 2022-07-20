using NoiseEngine.Logging;

namespace NoiseEngine;

public static class Log {

    public static Logger Logger { get; private set; } =
        new Logger(new ILogSink[] { new ConsoleLogSink(new ConsoleLogSinkSettings()) });

    /// <summary>
    /// Disposes old logger and replaces it with <paramref name="newLogger"/>.
    /// </summary>
    /// <param name="newLogger">New logger to use.</param>
    public static void ReplaceLogger(Logger newLogger) {
        lock (Logger) {
            Logger.Dispose();
            Logger = newLogger;
        }
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public static void Debug(string message) {
        Logger.Debug(message);
    }

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public static void Trace(string message) {
        Logger.Trace(message);
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public static void Info(string message) {
        Logger.Info(message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public static void Warning(string message) {
        Logger.Warning(message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public static void Error(string message) {
        Logger.Error(message);
    }

    /// <summary>
    /// Logs a fatal message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public static void Fatal(string message) {
        Logger.Fatal(message);
    }

}
