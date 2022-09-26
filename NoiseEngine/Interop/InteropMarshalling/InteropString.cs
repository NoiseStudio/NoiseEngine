using NoiseEngine.Interop.ResultErrors;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NoiseEngine.Interop.InteropMarshalling;

[StructLayout(LayoutKind.Sequential)]
internal struct InteropString : IDisposable, IResultError {

    private InteropArray<byte> array;

    [Obsolete("Use other constructor instead.", true)]
    public InteropString() {
        throw new InvalidOperationException();
    }

    public InteropString(string value) {
        int count = Encoding.UTF8.GetByteCount(value);
        array = new InteropArray<byte>(count);
        Encoding.UTF8.GetBytes(value, array.AsSpan());
    }

    public override string ToString() {
        return Encoding.UTF8.GetString(array.AsSpan());
    }

    public void Dispose() {
        array.Dispose();
    }

    Exception IResultError.ToException() {
        return new Exception(ToString());
    }

}
