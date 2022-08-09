using NoiseEngine.Collections;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Collections;

public class ComparableArrayTest {

    [Fact]
    public void DictionaryCompare() {
        ReadOnlySpan<object> span = new object[] {
            new object(), "Hello", 2107, 18.64f
        };

        object[] arrayA = span.ToArray();
        object[] arrayB = span.ToArray();

        Assert.NotSame(arrayA, arrayB);

        Dictionary<ComparableArray<object>, int> dictionary = new Dictionary<ComparableArray<object>, int>();

        dictionary[new ComparableArray<object>(arrayA)] = 7;
        dictionary[new ComparableArray<object>(arrayB)] = 11;

        Assert.Single(dictionary);
    }

    [Fact]
    public void ToArray() {
        object[] array = new object[] { new object(), "Hello", 2107, 18.64f };
        Assert.Same(array, new ComparableArray<object>(array).ToArray());
    }

    [Fact]
    public void GetEnumerator() {
        object[] array = new object[] { new object(), "Hello", 2107, 18.64f };
        Assert.Equal(array, new ComparableArray<object>(array));
    }

    [Fact]
    public void EqualsTest() {
        ReadOnlySpan<object> span = new object[] {
            new object(), "Hello", 2107, 18.64f
        };

        object[] arrayA = span.ToArray();
        object[] arrayB = span.ToArray();

        Assert.NotSame(arrayA, arrayB);

        Assert.Equal(new ComparableArray<object>(arrayA), new ComparableArray<object>(arrayB));
    }

    [Fact]
    public void EqualsObject() {
        ReadOnlySpan<object> span = new object[] {
            new object(), "Hello", 2107, 18.64f
        };

        object[] arrayA = span.ToArray();
        object[] arrayB = span.ToArray();

        Assert.NotSame(arrayA, arrayB);

        ComparableArray<object> comparableArray = new ComparableArray<object>(arrayA);

        Assert.True(comparableArray.Equals((object)new ComparableArray<object>(arrayB)));
        Assert.False(comparableArray.Equals(null));
        Assert.False(comparableArray!.Equals(54));
    }

}
