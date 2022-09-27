using NoiseEngine.Interop;
using NoiseEngine.Interop.ResultErrors;
using System;

namespace NoiseEngine.Tests.Interop.ResultErrors;

public partial class ResultErrorTest {

    [InteropImport("interop_result_errors_result_error_test_unmanaged_inner_error", InteropConstants.DllName)]
    private static partial ResultError InteropUnmanagedInnerError();

    [InteropImport("interop_result_errors_result_error_test_unmanaged_parse_bool", InteropConstants.DllName)]
    private static partial ResultError InteropUnmanagedParseBool();

    [Fact]
    public void InnerError() {
        using ResultError error = InteropUnmanagedInnerError();
        Exception exception = error.ToException();

        Assert.NotNull(exception.InnerException);
        Assert.Null(exception.InnerException!.InnerException);
    }

    [Fact]
    public void ParseBool() {
        using ResultError error = InteropUnmanagedParseBool();
        Exception exception = error.ToException();

        Assert.Null(exception.InnerException);
    }

}
