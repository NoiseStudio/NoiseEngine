using NoiseEngine.Interop;

namespace NoiseEngine.Tests.Interop;

public partial class InteropStringTest {

    private const string TestString = "Hello, world!";
    private const string TestStringNonAscii = "Hello, 世界!";

    [InteropImport("interop_interop_string_test_unmanaged_create", InteropConstants.DllName)]
    private static partial string InteropUnmanagedCreate();

    [InteropImport("interop_interop_string_test_unmanaged_create_from_string", InteropConstants.DllName)]
    private static partial string InteropUnmanagedCreateFromString();

    [InteropImport("interop_interop_string_test_unmanaged_destroy", InteropConstants.DllName)]
    private static partial void InteropUnmanagedDestroy(string s);

    [InteropImport("interop_interop_string_test_unmanaged_compare", InteropConstants.DllName)]
    private static partial bool InteropUnmanagedCompare(string s);

    [InteropImport("interop_interop_string_test_unmanaged_create_non_ascii", InteropConstants.DllName)]
    private static partial string InteropUnmanagedCreateNonAscii();

    [InteropImport("interop_interop_string_test_unmanaged_compare_non_ascii", InteropConstants.DllName)]
    private static partial bool InteropUnmanagedCompareNonAscii(string s);

    [Fact]
    public void UnmanagedCreate() {
        string s = InteropUnmanagedCreate();
        Assert.Equal(TestString, s);
    }

    [Fact]
    public void UnmanagedCreateFromString() {
        string s = InteropUnmanagedCreateFromString();
        Assert.Equal(TestString, s);
    }

    [Fact]
    public void UnmanagedDestroy() {
        InteropUnmanagedDestroy(TestString);
    }

    [Fact]
    public void UnmanagedCompare() {
        Assert.True(InteropUnmanagedCompare(TestString));
    }

    [Fact]
    public void UnmanagedCreateNonAscii() {
        string s = InteropUnmanagedCreateNonAscii();
        Assert.Equal(TestStringNonAscii, s);
    }

    [Fact]
    public void UnmanagedCompareNonAscii() {
        Assert.True(InteropUnmanagedCompareNonAscii(TestStringNonAscii));
    }

}
