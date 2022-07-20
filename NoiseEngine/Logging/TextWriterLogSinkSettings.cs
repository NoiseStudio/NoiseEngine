using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NoiseEngine.Logging;

public record TextWriterLogSinkSettings {

    protected static ImmutableArray<LogLevel> AllLevels { get; } = ImmutableArray.Create(
        LogLevel.Trace,
        LogLevel.Debug,
        LogLevel.Info,
        LogLevel.Warning,
        LogLevel.Error,
        LogLevel.Fatal);

    public bool IncludeTime { get; init; } = true;
    public bool IncludeLevel { get; init; } = true;
    public bool IncludeThreadName { get; init; } = true;

    public string TimeFormat { get; init; } = "HH:mm:ss.fff";

    public ImmutableDictionary<LogLevel, string> LevelToString { get; init; } = new Dictionary<LogLevel, string> {
        [LogLevel.Trace] = "Trace",
        [LogLevel.Debug] = "Debug",
        [LogLevel.Info] = "Info ",
        [LogLevel.Warning] = "Warn ",
        [LogLevel.Error] = "Error",
        [LogLevel.Fatal] = "Fatal",
    }.ToImmutableDictionary();

    /// <summary>
    /// If not null every thread name will be padded or truncated to be of this length.
    /// </summary>
    public int? ThreadNameMaxLength { get; init; } = null;

    /// <summary>
    /// Asserts that properties have valid values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Not all properties have valid values.</exception>
    public virtual void AssertValid() {
        foreach (LogLevel level in AllLevels) {
            if (!LevelToString.ContainsKey(level)) {
                throw new InvalidOperationException($"{nameof(LevelToString)} must contain a mapping for {level}.");
            }
        }
    }

}
