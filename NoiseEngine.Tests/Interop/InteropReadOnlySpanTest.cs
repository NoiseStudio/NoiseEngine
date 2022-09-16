using NoiseEngine.Interop;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Tests.Interop;

// TODO: Add managed read tests. It requires ownership of unmanaged allocated memory.
public partial class InteropReadOnlySpanTest {

    [LibraryImport(InteropConstants.DllName, EntryPoint = "interop_interop_read_only_span_test_unmanaged_read")]
    private static partial int InteropUnmanagedRead(InteropReadOnlySpan<int> span, int index);

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0)]
    [InlineData(new int[] { 1, 2 }, 1)]
    public void UnmanagedRead(int[] data, int index) {
        Assert.Equal(data[index], InteropUnmanagedRead(data.AsSpan(), index));
    }

}
