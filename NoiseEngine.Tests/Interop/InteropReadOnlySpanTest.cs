using NoiseEngine.Interop;
using System;

namespace NoiseEngine.Tests.Interop;

// TODO: Add managed read tests. It requires ownership of unmanaged allocated memory.
public partial class InteropReadOnlySpanTest {

    [InteropImport("interop_interop_read_only_span_test_unmanaged_read", InteropConstants.DllName)]
    private static partial int InteropUnmanagedRead(ReadOnlySpan<int> span, int index);

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0)]
    [InlineData(new int[] { 1, 2 }, 1)]
    public void UnmanagedRead(int[] data, int index) {
        Assert.Equal(data[index], InteropUnmanagedRead(data, index));
    }

}
