using System;
using System.Collections.Generic;

namespace NoiseEngine.Logging;

public class Logger {

    private readonly LoggerWorker worker;

    /// <summary>
    /// Sinks that this logger writes to. Items in this list are disposed when this object is disposed.
    /// </summary>
    public IEnumerable<ILogSink> Sinks => worker.Sinks;

    public LogLevel LogLevelMask { get; set; }

    /// <summary>
    /// Creates a new logger. The logger takes ownership of provided sinks and disposes them when it is disposed.
    /// </summary>
    /// <param name="sinks">Sinks to log to.</param>
    /// <param name="logLevelMask">Allowed log levels.</param>
    public Logger(IEnumerable<ILogSink> sinks, LogLevel logLevelMask = LogLevel.All & ~LogLevel.Trace) {
        worker = new LoggerWorker();

        foreach (ILogSink sink in sinks) {
            worker.Sinks.Add(sink);
        }

        LogLevelMask = logLevelMask;
    }

    ~Logger() {
        worker.Dispose();
    }

    private static bool CheckIfLogLevelIsSpecificValue(LogLevel level) {
        return level is
            LogLevel.Debug or
            LogLevel.Trace or
            LogLevel.Info or
            LogLevel.Warning or
            LogLevel.Error or
            LogLevel.Fatal;
    }

    /// <summary>
    /// Adds a sink to this logger. The logger takes ownership of the sink and disposes it when it is disposed.
    /// </summary>
    /// <param name="sink">Sink to add.</param>
    public void AddSink(ILogSink sink) {
        worker.Sinks.Add(sink);
    }

    /// <summary>
    /// Removes a sink from the logger. If removal was successful, caller is responsible for disposing the sink.
    /// </summary>
    /// <param name="sink">Sink to remove.</param>
    /// <returns><see langword="true"/> if sink was removed; otherwise <see langword="false"/>.</returns>
    public bool RemoveSink(ILogSink sink) {
        return worker.Sinks.Remove(sink);
    }

    /// <summary>
    /// Waits for all pending log messages to be written to sinks.
    /// </summary>
    public void Flush() {
        worker.Flush();
    }

    /// <summary>
    /// Logs data to all sinks.
    /// </summary>
    /// <param name="data">Data to log.</param>
    /// <exception cref="ArgumentOutOfRangeException">Log level is not a specific value.</exception>
    public void Log(LogData data) {
        if (!CheckIfLogLevelIsSpecificValue(data.Level)) {
            throw new ArgumentOutOfRangeException(
                nameof(data),
                data.Level,
                "Log level must be a specific value, not a mask or default.");
        }

        if ((data.Level & LogLevelMask) == LogLevel.None) {
            return;
        }

        worker.EnqueueLog(data);
    }

    /// <summary>
    /// Logs data to all sinks.
    /// </summary>
    /// <param name="level">Log level.</param>
    /// <param name="message">Message to log.</param>
    public void Log(LogLevel level, string message) {
        Log(new LogData(level, message));
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public void Debug(string message) {
        Log(LogLevel.Debug, message);
    }

    /// <summary>
    /// Logs a trace message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public void Trace(string message) {
        Log(LogLevel.Trace, message);
    }

    /// <summary>
    /// Logs an info message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public void Info(string message) {
        Log(LogLevel.Info, message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public void Warning(string message) {
        Log(LogLevel.Warning, message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public void Error(string message) {
        Log(LogLevel.Error, message);
    }

    /// <summary>
    /// Logs a fatal message.
    /// </summary>
    /// <param name="message">Message to log.</param>
    public void Fatal(string message) {
        Log(LogLevel.Fatal, message);
    }

}
