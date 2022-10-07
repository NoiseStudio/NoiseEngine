using System;

namespace NoiseEngine.Interop;

internal readonly record struct InteropHandle<T>(IntPtr Handle) {

    public static InteropHandle<T> Zero => new InteropHandle<T>(IntPtr.Zero);

    public bool IsNull => Handle == IntPtr.Zero;

    public bool Equals(InteropHandle<T> other) {
        return Handle == other.Handle;
    }

    public override int GetHashCode() {
        return Handle.GetHashCode();
    }

}
