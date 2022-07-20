using System;

namespace NoiseEngine.Logging;

public interface ILogSink : IDisposable {

    /// <summary>
    /// Writes a message to the sink. Calls to this method must be synchronized.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public void Log(LogData data);

}
