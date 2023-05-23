using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Nesl.Default;

internal static class Buffers {

    public const string RwBufferName = "System::System.RwBuffer`1";

    public static NeslType GetRwBuffer(NeslType type) {
        NeslType buffer = Manager.Assembly.GetType(RwBufferName) ?? throw new NullReferenceException();
        return buffer.MakeGeneric(type);
    }

}
