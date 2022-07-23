using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NoiseEngine.Logging;

public record ConsoleLogSinkSettings : TextWriterLogSinkSettings {

    public ImmutableDictionary<LogLevel, ConsoleColor> LevelToColor { get; init; } =
        new Dictionary<LogLevel, ConsoleColor> {
            [LogLevel.Debug] = ConsoleColor.Cyan,
            [LogLevel.Trace] = ConsoleColor.DarkGray,
            [LogLevel.Info] = ConsoleColor.White,
            [LogLevel.Warning] = ConsoleColor.Yellow,
            [LogLevel.Error] = ConsoleColor.Red,
            [LogLevel.Fatal] = ConsoleColor.Magenta
        }.ToImmutableDictionary();

    /// <summary>
    /// Asserts that properties have valid values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Not all properties have valid values.</exception>
    public override void AssertValid() {
        base.AssertValid();

        foreach (LogLevel level in AllLevels) {
            if (!LevelToColor.ContainsKey(level)) {
                throw new InvalidOperationException($"{nameof(LevelToColor)} must contain a mapping for {level}.");
            }
        }
    }

}
