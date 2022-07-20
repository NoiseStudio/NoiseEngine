using System.Collections.Generic;

namespace NoiseEngine.Logging;

public class LoggerBuilder {

    private readonly List<ILogSink> sinks = new List<ILogSink>();

    private LogLevel? logLevelMask;

    /// <summary>
    /// Sets the initial log level mask.
    /// </summary>
    /// <param name="mask">Mask to set.</param>
    /// <returns>This builder instance.</returns>
    public LoggerBuilder WithLogLevelMask(LogLevel mask) {
        logLevelMask = mask;
        return this;
    }

    /// <summary>
    /// Adds sink to the builder.
    /// </summary>
    /// <param name="sink">Sink to add.</param>
    /// <returns>This builder instance.</returns>
    public LoggerBuilder AddSink(ILogSink sink) {
        sinks.Add(sink);
        return this;
    }

    /// <summary>
    /// Builds a <see cref="Logger"/> instance.
    /// </summary>
    /// <returns>Built <see cref="Logger"/>.</returns>
    public Logger Build() {
        return logLevelMask is not null ? new Logger(sinks, logLevelMask.Value) : new Logger(sinks);
    }

}
