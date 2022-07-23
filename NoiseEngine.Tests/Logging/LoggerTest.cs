using System.Collections.Generic;
using System.Linq;
using NoiseEngine.Logging;

namespace NoiseEngine.Tests.Logging;

public class LoggerTest {

    [Fact]
    public void Log() {
        LogData[] data = {
            new LogData(LogLevel.Debug, "Debug"),
            new LogData(LogLevel.Trace, "Trace"),
            new LogData(LogLevel.Info, "Info"),
            new LogData(LogLevel.Warning, "Warning"),
            new LogData(LogLevel.Error, "Error"),
            new LogData(LogLevel.Fatal, "Fatal")
        };

        MockLogSink mockSink = new MockLogSink();
        Logger logger = new Logger(new ILogSink[] { mockSink }, LogLevel.All);

        foreach (LogData logData in data) {
            logger.Log(logData);
        }

        logger.Dispose();

        Assert.Equal(data.Length, mockSink.Logs.Count);
        Assert.Equal(data, mockSink.Logs);
    }

    [Fact]
    public void LevelMask() {
        LogData[] data = {
            new LogData(LogLevel.Debug, "Debug"),
            new LogData(LogLevel.Trace, "Trace"),
            new LogData(LogLevel.Info, "Info"),
            new LogData(LogLevel.Warning, "Warning"),
            new LogData(LogLevel.Error, "Error"),
            new LogData(LogLevel.Fatal, "Fatal")
        };

        const LogLevel Mask = LogLevel.Debug | LogLevel.Info | LogLevel.Error;

        MockLogSink mockSink = new MockLogSink();
        Logger logger = new Logger(new ILogSink[] { mockSink }, Mask);

        foreach (LogData logData in data) {
            logger.Log(logData);
        }

        logger.Dispose();

        IEnumerable<LogData> expected = data.Where(x => (x.Level & Mask) == x.Level);

        Assert.Equal(expected, mockSink.Logs);
    }

}
