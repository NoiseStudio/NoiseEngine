using NoiseEngine.Interop;

namespace NoiseEngine.Tests.Interop;

public partial class InteropOptionTest {

    [InteropImport("interop_interop_option_test_managed_read", InteropConstants.DllName)]
    private static partial InteropOption<int> InteropManagedRead(int value);

    [InteropImport("interop_interop_option_test_managed_read_none", InteropConstants.DllName)]
    private static partial InteropOption<int> InteropManagedReadNone();

    [InteropImport("interop_interop_option_test_unmanaged_read", InteropConstants.DllName)]
    private static partial int InteropUnmanagedRead(InteropOption<int> option);

    [InteropImport("interop_interop_option_test_unmanaged_read_none", InteropConstants.DllName)]
    private static partial bool InteropUnmanagedReadNone(InteropOption<int> option);

    [Theory]
    [InlineData(1864)]
    public void ManagedRead(int value) {
        InteropOption<int> option = InteropManagedRead(value);
        Assert.True(option.HasValue);
        Assert.Equal(value, option.Value);
    }

    [Fact]
    public void ManagedReadNone() {
        InteropOption<int> option = InteropManagedReadNone();
        Assert.False(option.HasValue);
    }

    [Theory]
    [InlineData(1864)]
    public void UnmanagedRead(int value) {
        Assert.Equal(value, InteropUnmanagedRead(value));
    }

    [Fact]
    public void UnmanagedReadNone() {
        Assert.True(InteropUnmanagedReadNone(null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(1864)]
    public void TryGetValue(int? value) {
        InteropOption<int> option = value;

        if (option.TryGetValue(out int v)) {
            Assert.NotNull(value);
            Assert.Equal(value!.Value, v);
        } else {
            Assert.Null(value);
        }
    }

    [Theory]
    [InlineData(null, 1864)]
    [InlineData(1864, null)]
    [InlineData(1864, 2107)]
    [InlineData(2107, 1864)]
    public void EqualsTest(int? valueA, int? valueB) {
        InteropOption<int> optionA = valueA;
        InteropOption<int> optionB = valueA;

        Assert.True(optionA.Equals(optionB));

        optionB = valueB;
        Assert.False(optionA.Equals(optionB));
    }

}
