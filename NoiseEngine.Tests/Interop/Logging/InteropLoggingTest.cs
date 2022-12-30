using System;
using System.Linq;
using NoiseEngine.Interop;
using NoiseEngine.Logging;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Logging;

namespace NoiseEngine.Tests.Interop.Logging;

[Collection(nameof(ApplicationCollection))]
public partial class InteropLoggingTest : IDisposable {

    private readonly MockLogSink sink;

    public InteropLoggingTest() {
        sink = new MockLogSink();
        Log.Logger.AddSink(sink);
    }

    [InteropImport("logging_interop_logging_test_debug")]
    private static partial void InteropDebug(string message);

    [InteropImport("logging_interop_logging_test_trace")]
    private static partial void InteropTrace(string message);

    [InteropImport("logging_interop_logging_test_info")]
    private static partial void InteropInfo(string message);

    [InteropImport("logging_interop_logging_test_warning")]
    private static partial void InteropWarning(string message);

    [InteropImport("logging_interop_logging_test_error")]
    private static partial void InteropError(string message);

    [InteropImport("logging_interop_logging_test_fatal")]
    private static partial void InteropFatal(string message);

    [Fact]
    public void Debug() {
        string message = Guid.NewGuid().ToString();
        InteropDebug(message);
        Log.Logger.Flush();
        LogData data = sink.Logs.ToArray().Single(x => x.Message == message);
        Assert.Equal(LogLevel.Debug, data.Level);
        Assert.Equal(message, data.Message);
    }

    [Fact]
    public void Trace() {
        string message = Guid.NewGuid().ToString();
        InteropTrace(message);
        Log.Logger.Flush();
        LogData data = sink.Logs.ToArray().Single(x => x.Message == message);
        Assert.Equal(LogLevel.Trace, data.Level);
        Assert.Equal(message, data.Message);
    }

    [Fact]
    public void Info() {
        string message = Guid.NewGuid().ToString();
        InteropInfo(message);
        Log.Logger.Flush();
        LogData data = sink.Logs.ToArray().Single(x => x.Message == message);
        Assert.Equal(LogLevel.Info, data.Level);
        Assert.Equal(message, data.Message);
    }

    [Fact]
    public void Warning() {
        string message = Guid.NewGuid().ToString();
        InteropWarning(message);
        Log.Logger.Flush();
        LogData data = sink.Logs.ToArray().Single(x => x.Message == message);
        Assert.Equal(LogLevel.Warning, data.Level);
        Assert.Equal(message, data.Message);
    }

    [Fact]
    public void Error() {
        string message = Guid.NewGuid().ToString();
        InteropError(message);
        Log.Logger.Flush();
        LogData data = sink.Logs.ToArray().Single(x => x.Message == message);
        Assert.Equal(LogLevel.Error, data.Level);
        Assert.Equal(message, data.Message);
    }

    [Fact]
    public void Fatal() {
        string message = Guid.NewGuid().ToString();
        InteropFatal(message);
        Log.Logger.Flush();
        LogData data = sink.Logs.ToArray().Single(x => x.Message == message);
        Assert.Equal(LogLevel.Fatal, data.Level);
        Assert.Equal(message, data.Message);
    }

    public void Dispose() {
        Log.Logger.RemoveSink(sink);
        sink.Dispose();
    }

}
