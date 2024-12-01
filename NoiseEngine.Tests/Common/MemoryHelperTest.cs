using System.Runtime.InteropServices;
using NoiseEngine.Common;

namespace NoiseEngine.Tests.Common;

public class MemoryHelperTest {

    [Fact]
    public void AlignmentOf() {
        nuint expected = (nuint)Marshal.SizeOf<int>();
        nuint actual = MemoryHelper.AlignmentOf<int>();
        
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public unsafe void IsDangling() {
        int* i = stackalloc int[1];
        *i = 420;

        Assert.False(MemoryHelper.IsDangling<int>((nuint)i));
    }
    
}
