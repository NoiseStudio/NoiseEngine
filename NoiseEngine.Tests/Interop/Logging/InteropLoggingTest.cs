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

    [InteropImport("logging_interop_logging_test_debug", InteropConstants.DllName)]
    private static partial void InteropDebug();

    [InteropImport("logging_interop_logging_test_trace", InteropConstants.DllName)]
    private static partial void InteropTrace();

    [InteropImport("logging_interop_logging_test_info", InteropConstants.DllName)]
    private static partial void InteropInfo();

    [InteropImport("logging_interop_logging_test_warning", InteropConstants.DllName)]
    private static partial void InteropWarning();

    [InteropImport("logging_interop_logging_test_error", InteropConstants.DllName)]
    private static partial void InteropError();

    [Fact]
    public void Debug() {
        InteropDebug();
        Log.Logger.Flush();
        LogData data = sink.Logs.Single();
        Assert.Equal(LogLevel.Debug, data.Level);
        Assert.Equal("debug", data.Message);
    }

    [Fact]
    public void Trace() {
        InteropTrace();
        Log.Logger.Flush();
        LogData data = sink.Logs.Single();
        Assert.Equal(LogLevel.Trace, data.Level);
        Assert.Equal("trace", data.Message);
    }

    [Fact]
    public void Info() {
        InteropInfo();
        Log.Logger.Flush();
        LogData data = sink.Logs.Single();
        Assert.Equal(LogLevel.Info, data.Level);
        Assert.Equal("info", data.Message);
    }

    [Fact]
    public void Warning() {
        InteropWarning();
        Log.Logger.Flush();
        LogData data = sink.Logs.Single();
        Assert.Equal(LogLevel.Warning, data.Level);
        Assert.Equal("warning", data.Message);
    }

    [Fact]
    public void Error() {
        InteropError();
        Log.Logger.Flush();
        LogData data = sink.Logs.Single();
        Assert.Equal(LogLevel.Error, data.Level);
        Assert.Equal("error", data.Message);
    }

    public void Dispose() {
        Log.Logger.RemoveSink(sink);
        sink.Dispose();
    }

}
