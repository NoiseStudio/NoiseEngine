using NoiseEngine.Interop;
using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Interop.ResultErrors;
using System;

namespace NoiseEngine.Tests.Interop.ResultErrors;

public partial class ResultErrorTest {

    [InteropImport("interop_result_errors_result_error_test_unmanaged_parse_bool", InteropConstants.DllName)]
    private static partial InteropResult<InteropBool, ResultError> InteropUnmanagedParseBool(string value);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ParseBoolValue(bool value) {
        InteropResult<InteropBool, ResultError> result = InteropUnmanagedParseBool(value.ToString().ToLower());
        Assert.Equal(value, result.Unwrap().Value);
    }

    [Fact]
    public void ParseBoolError() {
        InteropResult<InteropBool, ResultError> result = InteropUnmanagedParseBool("invalid");
        Exception exception = result.UnwrapError().ToException();

        Assert.Equal(typeof(FormatException), exception.GetType());
        Assert.Null(exception.InnerException);
    }

}
