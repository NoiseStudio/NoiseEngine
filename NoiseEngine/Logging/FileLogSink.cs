using System;
using System.IO;

namespace NoiseEngine.Logging;

public class FileLogSink : TextWriterLogSink {

    /// <summary>
    /// Creates a new <see cref="FileLogSink"/>.
    /// </summary>
    /// <param name="path">Path of the file to use. File is created if it does not exist.</param>
    /// <param name="settings">Sink settings, of which validity is asserted in the constructor.</param>
    public FileLogSink(string path, TextWriterLogSinkSettings settings) : base(new StreamWriter(path), settings) {
    }

    /// <summary>
    /// Creates a new <see cref="FileLogSink"/> in specified directory with timestamp as file name.
    /// </summary>
    /// <param name="directory">Directory to create a file in.</param>
    /// <param name="settings">Sink settings, of which validity is asserted. Null for default.</param>
    /// <returns>Sink writing to a file.</returns>
    public static FileLogSink CreateFromDirectory(string directory, TextWriterLogSinkSettings? settings = null) {
        settings ??= new TextWriterLogSinkSettings();

        Directory.CreateDirectory(directory);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string path = Path.Combine(directory, $"{timestamp}.log");
        return new FileLogSink(path, settings);
    }

}
