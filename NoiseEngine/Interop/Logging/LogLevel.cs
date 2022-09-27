namespace NoiseEngine.Interop.Logging;

internal enum LogLevel : byte {
    Debug = 1 << 0,
    Trace = 1 << 1,
    Info = 1 << 2,
    Warning = 1 << 3,
    Error = 1 << 4
}
