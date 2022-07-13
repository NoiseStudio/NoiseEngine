using NoiseEngine.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Tests.Collections.Concurrent;

public class ConcurrentHashSetTest {

    private readonly IEnumerable<int> hashSetValues = new int[] { 5, 6, 2, 5, 8, 2, 6, 7 };

    private ConcurrentHashSet<int> TestHashSet => new ConcurrentHashSet<int>(hashSetValues);
    private HashSet<int> TestDefaultHashSet => new HashSet<int>(hashSetValues);
    private IEnumerable<int> TestEnumerable => new int[] { 73, 23, 1, 6, 7, 3, 6 };

    [Fact]
    public void Add() {
        ConcurrentHashSet<int> hashSet = TestHashSet;

        Assert.DoesNotContain(-1, hashSet);
        Assert.True(hashSet.Add(-1));
        Assert.Contains(-1, hashSet);
    }

    [Fact]
    public void Remove() {
        ConcurrentHashSet<int> hashSet = TestHashSet;

        int value = hashSet.First();
        Assert.Contains(value, hashSet);
        Assert.True(hashSet.Remove(value));
        Assert.DoesNotContain(value, hashSet);
    }

    [Fact]
    public void Clear() {
        ConcurrentHashSet<int> hashSet = TestHashSet;
        hashSet.Clear();

        Assert.Empty(hashSet);
    }

    [Fact]
    public void CopyTo() {
        ConcurrentHashSet<int> hashSet = TestHashSet;
        int[] array = new int[hashSet.Count];

        hashSet.CopyTo(array, 0);
        Assert.Equal(hashSet, array);
    }

    [Fact]
    public void ExceptWith() {
        HashSet<int> defaultHashSet = TestDefaultHashSet;
        ConcurrentHashSet<int> hashSet = TestHashSet;

        defaultHashSet.ExceptWith(TestEnumerable);
        hashSet.ExceptWith(TestEnumerable);

        Assert.Equal(defaultHashSet.OrderBy(x => x), hashSet.OrderBy(x => x));
    }

    [Fact]
    public void IntersectWith() {
        HashSet<int> defaultHashSet = TestDefaultHashSet;
        ConcurrentHashSet<int> hashSet = TestHashSet;

        defaultHashSet.IntersectWith(TestEnumerable);
        hashSet.IntersectWith(TestEnumerable);

        Assert.Equal(defaultHashSet.OrderBy(x => x), hashSet.OrderBy(x => x));
    }

    [Fact]
    public void Overlaps() {
        HashSet<int> defaultHashSet = TestDefaultHashSet;
        ConcurrentHashSet<int> hashSet = TestHashSet;

        defaultHashSet.Overlaps(TestEnumerable);
        hashSet.Overlaps(TestEnumerable);

        Assert.Equal(defaultHashSet.OrderBy(x => x), hashSet.OrderBy(x => x));
    }

    [Fact]
    public void SetEquals() {
        HashSet<int> defaultHashSet = TestDefaultHashSet;
        ConcurrentHashSet<int> hashSet = TestHashSet;

        defaultHashSet.SetEquals(TestEnumerable);
        hashSet.SetEquals(TestEnumerable);

        Assert.Equal(defaultHashSet.OrderBy(x => x), hashSet.OrderBy(x => x));
    }

    [Fact]
    public void SymmetricExceptWith() {
        HashSet<int> defaultHashSet = TestDefaultHashSet;
        ConcurrentHashSet<int> hashSet = TestHashSet;

        defaultHashSet.SymmetricExceptWith(TestEnumerable);
        hashSet.SymmetricExceptWith(TestEnumerable);

        Assert.Equal(defaultHashSet.OrderBy(x => x), hashSet.OrderBy(x => x));
    }

    [Fact]
    public void UnionWith() {
        HashSet<int> defaultHashSet = TestDefaultHashSet;
        ConcurrentHashSet<int> hashSet = TestHashSet;

        defaultHashSet.UnionWith(TestEnumerable);
        hashSet.UnionWith(TestEnumerable);

        Assert.Equal(defaultHashSet.OrderBy(x => x), hashSet.OrderBy(x => x));
    }

}
