using NoiseEngine.Collections;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Collections;

public class ComparableReadOnlyListTest {

    private readonly object mockObject = new object();

    [Fact]
    public void DictionaryCompare() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotSame(arrayA, arrayB);

        Dictionary<ComparableReadOnlyList<object>, int> dictionary =
            new Dictionary<ComparableReadOnlyList<object>, int>();

        dictionary[new ComparableReadOnlyList<object>(arrayA)] = 7;
        dictionary[new ComparableReadOnlyList<object>(arrayB)] = 11;

        Assert.Single(dictionary);
    }

    [Fact]
    public void GetEnumerator() {
        object[] array = GetNewTestArray();
        Assert.Equal(array, new ComparableReadOnlyList<object>(array));
    }

    [Fact]
    public void EqualsTest() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotSame(arrayA, arrayB);

        Assert.Equal(new ComparableReadOnlyList<object>(arrayA), new ComparableReadOnlyList<object>(arrayB));
    }

    [Fact]
    public void EqualsObject() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotSame(arrayA, arrayB);

        ComparableReadOnlyList<object> comparableArray = new ComparableReadOnlyList<object>(arrayA);

        Assert.True(comparableArray.Equals((object)new ComparableReadOnlyList<object>(arrayB)));
        Assert.False(comparableArray.Equals(null));
        Assert.False(comparableArray!.Equals(54));
    }

    private object[] GetNewTestArray() {
        return new object[] { mockObject, "Hello", 2107, 18.64f };
    }

}
