﻿using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct InteropHandle<T>(IntPtr Pointer) {

    public static InteropHandle<T> Zero => new InteropHandle<T>(IntPtr.Zero);

    private static readonly unsafe string ToStringFormat = "x" + sizeof(IntPtr) * 2;

    public bool IsNull => Pointer == IntPtr.Zero;

    public bool Equals(InteropHandle<T> other) {
        return Pointer == other.Pointer;
    }

    public override int GetHashCode() {
        return Pointer.GetHashCode();
    }

    public override string ToString() {
        return $"0x{Pointer.ToString(ToStringFormat)}";
    }

}
