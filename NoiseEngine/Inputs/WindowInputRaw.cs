using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Inputs;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct WindowInputRaw {

    private fixed uint keyValues[133];

    public Vector2<double> CursorPosition { get; }
    public Vector2<double> ScrollDelta { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public KeyValue GetKeyValue(int index) {
        fixed (uint* pointer = &keyValues[index])
            return ((KeyValue*)pointer)[0];
    }

}
