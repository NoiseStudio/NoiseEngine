using System;

namespace NoiseEngine.Logging;

public class ConsoleLogSink : TextWriterLogSink {

    private readonly ConsoleLogSinkSettings settings;

    public ConsoleLogSink(ConsoleLogSinkSettings settings) : base(Console.Out, settings) {
        this.settings = settings;
    }

    /// <summary>
    /// Writes a message to the console.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public override void Log(LogData data) {
        lock (Console.Out) {
            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = settings.LevelToColor[data.Level];
            base.Log(data);
            Console.ForegroundColor = color;
        }
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public override void Dispose() {
    }

}
