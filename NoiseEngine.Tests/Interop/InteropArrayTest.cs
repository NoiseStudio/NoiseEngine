using NoiseEngine.Interop;
using System;
using System.Linq;

namespace NoiseEngine.Tests.Interop;

public partial class InteropArrayTest {

    [InteropImport("interop_interop_array_test_unmanaged_create", InteropConstants.DllName)]
    private static partial InteropArray<int> InteropUnmanagedCreate(int length);

    [InteropImport("interop_interop_array_test_unmanaged_create_from_vec", InteropConstants.DllName)]
    private static partial InteropArray<int> InteropUnmanagedCreateFromVec(int length);

    [InteropImport("interop_interop_array_test_unmanaged_destroy", InteropConstants.DllName)]
    private static partial void InteropUnmanagedDestroy(InteropArray<int> array);

    [InteropImport("interop_interop_array_test_unmanaged_destroy_vec", InteropConstants.DllName)]
    private static partial void InteropUnmanagedDestroyVec(InteropArray<int> array);

    [InteropImport("interop_interop_array_test_unmanaged_read", InteropConstants.DllName)]
    private static partial int InteropUnmanagedRead(in InteropArray<int> span, int index);

    [InteropImport("interop_interop_array_test_unmanaged_write", InteropConstants.DllName)]
    private static partial void InteropUnmanagedWrite(in InteropArray<int> span, int index, int value);

    [InteropImport("interop_interop_array_test_unmanaged_as_slice", InteropConstants.DllName)]
    private static partial InteropArray<int> InteropUnmanagedAsSlice(InteropArray<int> array);

    [InteropImport("interop_interop_array_test_unmanaged_as_mut_slice", InteropConstants.DllName)]
    private static partial InteropArray<int> InteropUnmanagedAsMutSlice(InteropArray<int> array);

    [Theory]
    [InlineData(42)]
    public void UnmanagedCreate(int length) {
        using InteropArray<int> array = InteropUnmanagedCreate(length);
        Assert.Equal(length, array.Length);
        Assert.Equal(Enumerable.Range(0, length), array.AsSpan().ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void UnmanagedCreateFromVec(int length) {
        using InteropArray<int> array = InteropUnmanagedCreateFromVec(length);
        Assert.Equal(length, array.Length);
        Assert.Equal(Enumerable.Range(0, length), array.AsSpan().ToArray());
    }

    [Theory]
    [InlineData(new int[] { 1, 2 })]
    public void UnmanagedDestroy(int[] array) {
        InteropArray<int> interopArray = new InteropArray<int>(array);
        InteropUnmanagedDestroy(interopArray);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 })]
    public void UnmanagedDestroyVec(int[] array) {
        InteropArray<int> interopArray = new InteropArray<int>(array);
        InteropUnmanagedDestroyVec(interopArray);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0)]
    [InlineData(new int[] { 1, 2 }, 1)]
    public void UnmanagedRead(int[] data, int index) {
        using InteropArray<int> interopData = new InteropArray<int>(data);
        Assert.Equal(data[index], InteropUnmanagedRead(interopData, index));
    }

    [Theory]
    [InlineData(new int[] { 1, 2 }, 0, 7)]
    [InlineData(new int[] { 1, 2 }, 1, 3)]
    public void UnmanagedWrite(int[] data, int index, int value) {
        using InteropArray<int> interopData = new InteropArray<int>(data);
        InteropUnmanagedWrite(interopData, index, value);
        Assert.Equal(value, interopData[index]);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 })]
    public void UnmanagedAsSlice(int[] array) {
        Assert.Equal(array, InteropUnmanagedAsSlice(new InteropArray<int>(array)));
    }

    [Theory]
    [InlineData(new int[] { 1, 2 })]
    public void UnmanagedAsMutSlice(int[] array) {
        Assert.Equal(array, InteropUnmanagedAsMutSlice(new InteropArray<int>(array)));
    }

    [Theory]
    [InlineData(42)]
    public void CreateFromLength(int length) {
        using InteropArray<int> array = new InteropArray<int>(length);
        Assert.Equal(length, array.Length);
    }

    [Theory]
    [InlineData(new int[] { 1, 2 })]
    public void CreateFromArray(int[] data) {
        using InteropArray<int> array = new InteropArray<int>(data);
        Assert.Equal(data, array.AsSpan().ToArray());
    }

    [Theory]
    [InlineData(new int[] { 1, 2 })]
    public void CreateFromSpan(int[] data) {
        using InteropArray<int> array = new InteropArray<int>(data.AsSpan());
        Assert.Equal(data, array.AsSpan().ToArray());
    }

    [Theory]
    [InlineData(new int[] { 1, 2, 3, 4 }, 1, 2)]
    public void AsSpan(int[] data, int start, int length) {
        using InteropArray<int> array = new InteropArray<int>(data);
        Span<int> span = array.AsSpan(start, length);

        Assert.Equal(data.AsSpan(start, length).ToArray(), span.ToArray());
    }

}
