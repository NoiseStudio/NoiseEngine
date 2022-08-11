using NoiseEngine.Collections;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Collections;

public class EquatableReadOnlyListTest {

    private readonly object mockObject = new object();

    [Fact]
    public void DictionaryCompare() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotSame(arrayA, arrayB);

        Dictionary<EquatableReadOnlyList<object>, int> dictionary =
            new Dictionary<EquatableReadOnlyList<object>, int>();

        dictionary[new EquatableReadOnlyList<object>(arrayA)] = 7;
        dictionary[new EquatableReadOnlyList<object>(arrayB)] = 11;

        Assert.Single(dictionary);
    }

    [Fact]
    public void GetEnumerator() {
        object[] array = GetNewTestArray();
        Assert.Equal(array, new EquatableReadOnlyList<object>(array));
    }

    [Fact]
    public void EqualsTest() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotSame(arrayA, arrayB);

        Assert.Equal(new EquatableReadOnlyList<object>(arrayA), new EquatableReadOnlyList<object>(arrayB));
    }

    [Fact]
    public void EqualsObject() {
        object[] arrayA = GetNewTestArray();
        object[] arrayB = GetNewTestArray();

        Assert.NotSame(arrayA, arrayB);

        EquatableReadOnlyList<object> comparableArray = new EquatableReadOnlyList<object>(arrayA);

        Assert.True(comparableArray.Equals((object)new EquatableReadOnlyList<object>(arrayB)));
        Assert.False(comparableArray.Equals(null));
        Assert.False(comparableArray!.Equals(54));
    }

    private object[] GetNewTestArray() {
        return new object[] { mockObject, "Hello", 2107, 18.64f };
    }

}
