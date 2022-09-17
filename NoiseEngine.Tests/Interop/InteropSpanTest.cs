using NoiseEngine.Interop;
using System;

namespace NoiseEngine.Tests.Interop;

// TODO: Add managed read and write tests. It requires ownership of unmanaged allocated memory.
public partial class InteropSpanTest {

    [InteropImport("interop_interop_span_test_unmanaged_read", InteropConstants.DllName)]
    private static partial int InteropUnmanagedRead(Span<int> span, int index);

    [InteropImport("interop_interop_span_test_unmanaged_write", InteropConstants.DllName)]
    private static partial void InteropUnmanagedWrite(Span<int> span, int index, int value);

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0)]
    [InlineData(new int[] { 1, 2 }, 1)]
    public void UnmanagedRead(int[] data, int index) {
        Assert.Equal(data[index], InteropUnmanagedRead(data, index));
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0, 7)]
    [InlineData(new int[] { 1, 2 }, 1, 3)]
    public void UnmanagedWrite(int[] data, int index, int value) {
        InteropUnmanagedWrite(data, index, value);
        Assert.Equal(value, data[index]);
    }

}
