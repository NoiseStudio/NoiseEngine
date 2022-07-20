using System;
using System.Threading;

namespace NoiseEngine.Logging;

public record struct LogData(LogLevel Level, string Message, DateTime Time, string? ThreadName) {

    public LogData(LogLevel level, string message) : this(level, message, DateTime.Now, Thread.CurrentThread.Name) {
    }

}
