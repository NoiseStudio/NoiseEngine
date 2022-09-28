using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NoiseEngine.Logging;

namespace NoiseEngine.Tests.Logging;

public class LoggerTest {

    private readonly ImmutableArray<LogData> data = ImmutableArray.Create(
        new LogData(LogLevel.Debug, "Debug"),
        new LogData(LogLevel.Trace, "Trace"),
        new LogData(LogLevel.Info, "Info"),
        new LogData(LogLevel.Warning, "Warning"),
        new LogData(LogLevel.Error, "Error"),
        new LogData(LogLevel.Fatal, "Fatal")
    );

    [Fact]
    public void Log() {
        MockLogSink mockSink = new MockLogSink();

        using (Logger logger = new Logger(new ILogSink[] { mockSink }, LogLevel.All)) {
            foreach (LogData logData in data) {
                logger.Log(logData);
            }
        }

        Assert.Equal(data, mockSink.Logs);
    }

    [Fact]
    public void Flush() {
        MockLogSink mockSink = new MockLogSink();

        using Logger logger = new Logger(new ILogSink[] { mockSink }, LogLevel.All);

        foreach (LogData logData in data) {
            logger.Log(logData);
        }

        logger.Flush();
        Assert.Equal(data, mockSink.Logs);
    }

    [Fact]
    public void LevelMask() {
        const LogLevel Mask = LogLevel.Debug | LogLevel.Info | LogLevel.Error;

        MockLogSink mockSink = new MockLogSink();

        using (Logger logger = new Logger(new ILogSink[] { mockSink }, Mask)) {
            foreach (LogData logData in data) {
                logger.Log(logData);
            }
        }

        IEnumerable<LogData> expected = data.Where(x => (x.Level & Mask) == x.Level);

        Assert.Equal(expected, mockSink.Logs);
    }

}
