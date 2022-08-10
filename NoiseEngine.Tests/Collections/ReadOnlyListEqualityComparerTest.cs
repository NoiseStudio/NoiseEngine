using NoiseEngine.Collections;

namespace NoiseEngine.Tests.Collections;

public class ReadOnlyListEqualityComparerTest {

    private readonly object mockObject = new object();

    [Fact]
    public void EqualsTest() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotSame(arrayA, arrayB);

        ReadOnlyListEqualityComparer<object> comparer = new ReadOnlyListEqualityComparer<object>();
        Assert.True(comparer.Equals(arrayA, arrayB));
    }

    [Fact]
    public void GetHashCodeTest() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotEqual(arrayA.GetHashCode(), arrayB.GetHashCode());

        ReadOnlyListEqualityComparer<object> comparer = new ReadOnlyListEqualityComparer<object>();
        Assert.Equal(comparer.GetHashCode(arrayA), comparer.GetHashCode(arrayB));
    }

    private object[] GetNewTestArray() {
        return new object[] { mockObject, "Hello", 2107, 18.64f };
    }

}
