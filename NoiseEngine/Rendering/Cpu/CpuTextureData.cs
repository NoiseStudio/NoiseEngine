using System.Runtime.InteropServices;
using NoiseEngine.Interop;

namespace NoiseEngine.Rendering.Cpu;

[StructLayout(LayoutKind.Sequential)]
internal struct CpuTextureData {

    public uint ExtentX { get; init; }
    public uint ExtentY { get; init; }
    public uint ExtentZ { get; init; }
    public CpuTextureFormat Format { get; init; }
    public InteropArray<byte> Data { get; init; }

}
