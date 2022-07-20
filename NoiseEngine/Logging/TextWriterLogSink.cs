using System.IO;
using System.Text;

namespace NoiseEngine.Logging;

public class TextWriterLogSink : ILogSink {

    private readonly TextWriter writer;
    private readonly TextWriterLogSinkSettings settings;

    /// <summary>
    /// Creates a new <see cref="TextWriterLogSink"/>. Takes ownership of the provided <paramref name="writer"/>
    /// and disposes it when this object is disposed.
    /// </summary>
    /// <param name="writer">Writer to use.</param>
    /// <param name="settings">Sink settings, of which validity is asserted in the constructor.</param>
    public TextWriterLogSink(TextWriter writer, TextWriterLogSinkSettings settings) {
        settings.AssertValid();

        this.writer = writer;
        this.settings = settings;
    }

    private static string EnsureLength(string? text, int length) {
        if (text is null) {
            return new StringBuilder(length).Append(' ', length).ToString();
        }

        if (text.Length > length) {
            return text[..length];
        }

        if (text.Length < length) {
            return new StringBuilder(text, length).Append(' ', length - text.Length).ToString();
        }

        return text;
    }

    /// <summary>
    /// Writes a message to the text writer. Calls to this method must be synchronized.
    /// </summary>
    /// <param name="data">Data to log.</param>
    public virtual void Log(LogData data) {
        if (settings.IncludeTime) {
            writer.Write('[');
            writer.Write(data.Time.ToString(settings.TimeFormat));
            writer.Write("] ");
        }

        if (settings.IncludeLevel) {
            writer.Write('[');
            writer.Write(settings.LevelToString[data.Level]);
            writer.Write("] ");
        }

        if (settings.IncludeThreadName) {
            writer.Write('[');

            writer.Write(
                settings.ThreadNameMaxLength is null
                    ? data.ThreadName
                    : EnsureLength(data.ThreadName, settings.ThreadNameMaxLength.Value));

            writer.Write("] ");
        }

        writer.Write(data.Message);
        writer.WriteLine();
    }

    /// <summary>
    /// Disposes this object.
    /// </summary>
    public virtual void Dispose() {
        writer.Dispose();
    }

}
