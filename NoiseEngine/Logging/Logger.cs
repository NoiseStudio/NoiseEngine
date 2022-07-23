using System;
using System.Collections.Generic;
using NoiseEngine.Collections.Concurrent;

namespace NoiseEngine.Logging;

public class Logger : IDisposable {

    private readonly LoggerWorker worker;

    /// <summary>
    /// Sinks that this logger writes to. Items in this list are disposed when this object is disposed.
    /// </summary>
    public ConcurrentList<ILogSink> Sinks => worker.Sinks;

    public LogLevel LogLevelMask { get; set; }

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Creates a new logger. The logger takes ownership of provided sinks and disposes them when it is disposed.
    /// </summary>
    /// <param name="sinks">Sinks to log to.</param>
    /// <param name="logLevelMask">Allowed log levels.</param>
    public Logger(IEnumerable<ILogSink> sinks, LogLevel logLevelMask = LogLevel.All & ~LogLevel.Trace) {
        worker = new LoggerWorker();

        foreach (ILogSink sink in sinks) {
            Sinks.Add(sink);
        }

        LogLevelMask = logLevelMask;
    }

    ~Logger() {
        Warning("Logger has not been disposed explicitly.");
        ReleaseResources();
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

    /// <summary>
    /// Disposes the logger and all the sinks.
    /// First time this method is called it waits for the worker thread to finish,
    /// subsequent calls do not have that guarantee.
    /// </summary>
    public void Dispose() {
        if (IsDisposed) {
            return;
        }

        IsDisposed = true;
        ReleaseResources();
        GC.SuppressFinalize(this);
    }

    private void ReleaseResources() {
        worker.Dispose();
    }

}
