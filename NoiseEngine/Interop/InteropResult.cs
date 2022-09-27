using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Interop.ResultErrors;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct InteropResult<TValue, TError>
    where TValue : unmanaged
    where TError : unmanaged, IResultError
{

    private readonly InteropBool isOk;
    private readonly TValue value;
    private readonly TError error;

    public bool IsOk => isOk;

    public bool TryGetValue(out TValue value, out TError error) {
        value = this.value;
        error = this.error;
        return IsOk;
    }

    public TValue Unwrap() {
        if (IsOk)
            return value;

        Exception exception = error.ToException();
        if (error is IDisposable disposable)
            disposable.Dispose();

        throw new InvalidOperationException(
            $"Unable to unwrap value from {nameof(InteropResult<TValue, TError>)} which is not ok.", exception);
    }

    public TError UnwrapError() {
        if (!IsOk)
            return error;

        if (value is IDisposable disposable)
            disposable.Dispose();

        throw new InvalidOperationException(
            $"Unable to unwrap error from {nameof(InteropResult<TValue, TError>)} which is ok.");
    }

}
