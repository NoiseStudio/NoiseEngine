using System.Collections.Generic;
using NoiseEngine.Logging;

namespace NoiseEngine.Tests.Logging;

public class MockLogSink : ILogSink {

    public List<LogData> Logs { get; } = new List<LogData>();

    public void Dispose() {
    }

    public void Log(LogData data) {
        Logs.Add(data);
    }

}
