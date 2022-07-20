using NoiseEngine.Logging;

namespace NoiseEngine;

public static class Log {

    public static Logger Logger { get; set; } = new Logger(new ILogSink[0]);

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
