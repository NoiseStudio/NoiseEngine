using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal struct InteropResult<T> where T : unmanaged {

    private readonly InteropBool isOk;
    private T value;
    private ResultError error;

    public bool IsOk => isOk;

    public T Value {
        get {
            if (IsOk)
                return value;

            Exception exception = error.ToException();
            error.Dispose();

            throw new InvalidOperationException(
                $"Unable to unwrap value from {nameof(InteropResult<T>)} which is not ok.", exception);
        }
    }

    public ResultError Error {
        get {
            if (!IsOk)
                return error;

            if (value is IDisposable disposable) {
                disposable.Dispose();
                value = (T)disposable;
            }

            throw new InvalidOperationException(
                $"Unable to unwrap error from {nameof(InteropResult<T>)} which is ok.");
        }
    }

    public bool TryGetValue(out T value, out ResultError error) {
        value = this.value;
        error = this.error;
        return IsOk;
    }

}
