using NoiseEngine.Interop.InteropMarshalling;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Interop.ResultErrors;

[StructLayout(LayoutKind.Sequential)]
internal record struct ResultError : IDisposable, IResultError {

    private readonly InteropString message;
    private IntPtr sourcePointer;

    public ResultErrorKind Kind { get; }
    public string Message => message.ToString();
    public ResultError? InnerError {
        get {
            if (sourcePointer == IntPtr.Zero)
                return null;
            return Marshal.PtrToStructure<ResultError>(sourcePointer);
        }
    }

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

        return Kind switch {
            ResultErrorKind.Universal => new Exception(Message, innerException),
            _ => throw new NotImplementedException()
        };
    }

}
