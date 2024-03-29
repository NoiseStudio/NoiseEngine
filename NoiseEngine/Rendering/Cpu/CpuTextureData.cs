﻿using System;
using System.Runtime.InteropServices;
using NoiseEngine.Interop;

namespace NoiseEngine.Rendering.Cpu;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct CpuTextureData : IDisposable {

    public uint ExtentX { get; init; }
    public uint ExtentY { get; init; }
    public uint ExtentZ { get; init; }
    public TextureFormat Format { get; init; }
    public InteropArray<byte> Data { get; init; }

    public void Dispose() {
        Data.Dispose();
    }

}
