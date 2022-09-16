using NoiseEngine.Interop;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Tests.Interop;

// TODO: Add managed read and write tests. It requires ownership of unmanaged allocated memory.
public partial class InteropSpanTest {

    [LibraryImport(InteropConstants.DllName, EntryPoint = "interop_interop_span_test_unmanaged_read")]
    private static partial int InteropUnmanagedRead(InteropSpan<int> span, int index);

    [LibraryImport(InteropConstants.DllName, EntryPoint = "interop_interop_span_test_unmanaged_write")]
    private static partial void InteropUnmanagedWrite(InteropSpan<int> span, int index, int value);

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0)]
    [InlineData(new int[] { 1, 2 }, 1)]
    public void UnmanagedRead(int[] data, int index) {
        Assert.Equal(data[index], InteropUnmanagedRead(data.AsSpan(), index));
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0, 7)]
    [InlineData(new int[] { 1, 2 }, 1, 3)]
    public void UnmanagedWrite(int[] data, int index, int value) {
        InteropUnmanagedWrite(data.AsSpan(), index, value);
        Assert.Equal(value, data[index]);
    }

}
