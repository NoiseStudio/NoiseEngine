using System.Runtime.InteropServices;
using NoiseEngine.Interop;

namespace NoiseEngine.Tests.Interop;

public partial class InteropLibTest {

    [LibraryImport(InteropConstants.DllName, EntryPoint = "return_42")]
    private static partial int Return42();

    [Theory]
    [InlineData(6, 9)]
    public void Add(int left, int right) {
        Assert.Equal(left + right, InteropLib.Add(left, right));
    }

    [Fact]
    public void Return42IsEqualTo42() {
        Assert.Equal(42, Return42());
    }

}
