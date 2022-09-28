using NoiseEngine.Interop;
using System;

namespace NoiseEngine.Tests.Interop;

public partial class InteropResultTest {

    private const int Value = 6582;
    private const string ErrorValue = "Mn";

    [InteropImport("interop_interop_result_test_unmanaged_create_value", InteropConstants.DllName)]
    private static partial InteropResult<int> InteropUnmanagedCreateValue(int value);

    [InteropImport("interop_interop_result_test_unmanaged_create_error", InteropConstants.DllName)]
    private static partial InteropResult<int> InteropUnmanagedCreateError(string value);

    [Fact]
    public void TryGetValueValue() {
        InteropResult<int> result = InteropUnmanagedCreateValue(Value);
        Assert.True(result.TryGetValue(out int value, out _));
        Assert.Equal(Value, value);
    }

    [Fact]
    public void TryGetValueError() {
        InteropResult<int> result = InteropUnmanagedCreateError(ErrorValue);
        Assert.False(result.TryGetValue(out _, out ResultError error));
        Assert.Equal(ErrorValue, error.Message);
        error.Dispose();
    }

    [Fact]
    public void UnwrapValue() {
        InteropResult<int> result = InteropUnmanagedCreateValue(Value);
        Assert.Equal(Value, result.Unwrap());
    }

    [Fact]
    public void UnwrapError() {
        InteropResult<int> result = InteropUnmanagedCreateError(ErrorValue);
        Assert.Throws<InvalidOperationException>(() => result.Unwrap());
    }

    [Fact]
    public void UnwrapErrorValue() {
        InteropResult<int> result = InteropUnmanagedCreateValue(Value);
        Assert.Throws<InvalidOperationException>(() => result.UnwrapError());
    }

    [Fact]
    public void UnwrapErrorError() {
        InteropResult<int> result = InteropUnmanagedCreateError(ErrorValue);
        using ResultError error = result.UnwrapError();
        Assert.Equal(ErrorValue, error.Message);
    }

}
