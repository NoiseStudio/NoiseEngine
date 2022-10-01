using System.Runtime.InteropServices;
using NoiseEngine.Interop.InteropMarshalling;

namespace NoiseEngine.Interop.Logging;

[StructLayout(LayoutKind.Sequential)]
internal ref struct LogData {

    public LogLevel Level { get; init; }
    public InteropReadOnlySpan<byte> Message { get; init; }

}
