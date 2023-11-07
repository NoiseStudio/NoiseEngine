using NoiseEngine.Interop;

namespace NoiseEngine.Tests.Interop;

public partial class InteropMemoryHelperTest {

    [InteropImport("interop_interop_memory_helper_test_alignment_of_i32", InteropConstants.DllName)]
    private static partial nuint InteropAlignmentOfI32();

    [InteropImport("interop_interop_memory_helper_test_alignment_of_u8", InteropConstants.DllName)]
    private static partial nuint InteropAlignmentOfU8();

    [InteropImport("interop_interop_memory_helper_test_alignment_of_struct_a", InteropConstants.DllName)]
    private static partial nuint InteropAlignmentOfStructA();

    [Fact]
    public void AlignmentOfI32() {
        Assert.Equal(InteropAlignmentOfI32(), InteropMemoryHelper.AlignmentOf<int>());
    }

    [Fact]
    public void AlignmentOfU8() {
        Assert.Equal(InteropAlignmentOfU8(), InteropMemoryHelper.AlignmentOf<byte>());
    }

    [Fact]
    public void AlignmentOfStructA() {
        Assert.Equal(InteropAlignmentOfStructA(), InteropMemoryHelper.AlignmentOf<AlignmentTestStructA>());
    }

}
