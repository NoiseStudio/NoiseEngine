using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.ResultErrors;

internal record struct ResultError : IDisposable, IResultError {

    private readonly ResultErrorKind kind;
    private readonly InteropString message;
    private IntPtr sourcePointer;

    public void Dispose() {
        message.Dispose();

        if (sourcePointer == IntPtr.Zero)
            return;

        ResultError source = Marshal.PtrToStructure<ResultError>(sourcePointer);
        Marshal.FreeHGlobal(sourcePointer);
        sourcePointer = IntPtr.Zero;

        source.Dispose();
    }

    public Exception ToException() {
        Exception? innerException;
        if (sourcePointer != IntPtr.Zero)
            innerException = Marshal.PtrToStructure<ResultError>(sourcePointer).ToException();
        else
            innerException = null;

        return kind switch {
            ResultErrorKind.Universal => new Exception(message.ToString(), innerException),
            ResultErrorKind.Format => new FormatException(message.ToString(), innerException),
            _ => throw new NotImplementedException()
        };
    }

}
