using NoiseEngine.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Collections.Concurrent;

public class ConcurrentListTest {

    private IReadOnlyCollection<int> TestArray { get; } =
        Enumerable.Range(0, Environment.ProcessorCount * 5).ToArray();

    [Fact]
    public void ConstructorIEnumerable() {
        Assert.Equal(TestArray, new ConcurrentList<int>(TestArray.AsEnumerable()).OrderBy(x => x));
    }

    [Fact]
    public void ConstructorReadOnlySpan() {
        ReadOnlySpan<int> span = stackalloc int[] { 5, 7, 2, -6, 1 };
        Assert.Equal(span.ToArray().OrderBy(x => x), new ConcurrentList<int>(span).OrderBy(x => x));
    }

    [Fact]
    public void Add() {
        ConcurrentList<int> list = new ConcurrentList<int>();

        Parallel.ForEach(TestArray, list.Add);

        Assert.Equal(TestArray, list.OrderBy(x => x));
    }

    [Fact]
    public void AddRangeSpan() {
        ConcurrentList<int> list = new ConcurrentList<int>();
        List<int> testList = new List<int>();

        for (int i = 0; i < Environment.ProcessorCount; i++)
            testList.AddRange(TestArray);

        Parallel.For(0, Environment.ProcessorCount, _ => list.AddRange(TestArray.ToArray().AsSpan()));

        Assert.Equal(testList.OrderBy(x => x), list.OrderBy(x => x));
    }

    [Fact]
    public void Clear() {
        ConcurrentList<int> list = new ConcurrentList<int>();
        list.Add(4);

        list.Clear();
        Assert.Empty(list);
    }

    [Fact]
    public void Contains() {
        ConcurrentList<int> list = new ConcurrentList<int>();

        Parallel.ForEach(TestArray, x => {
            list.Add(x);
            Assert.Contains(x, list);
        });
    }

    [Fact]
    public void CopyTo() {
        ConcurrentList<bool> list = new ConcurrentList<bool>();
        list.Add(true);

        bool[] array = new bool[1];
        list.CopyTo(array, 0);

        Assert.True(array[0]);
    }

    [Fact]
    public void GetEnumerator() {
        ConcurrentList<int> list = new ConcurrentList<int>();
        list.Add(5);
        list.Add(4);
        list.Add(3);

        int sum = 0;
        foreach (int item in list)
            sum += item;

        Assert.Equal(12, sum);
    }

    [Fact]
    public void Remove() {
        ConcurrentList<int> list = new ConcurrentList<int>();

        int[] chunk = TestArray.Chunk(TestArray.Count / 2).First();
        List<int> testList = new List<int>(TestArray);

        foreach (int element in chunk)
            testList.Remove(element);

        Parallel.ForEach(TestArray, list.Add);
        Parallel.ForEach(chunk, x => list.Remove(x));

        Assert.Equal(testList.OrderBy(x => x), list.OrderBy(x => x));
    }

}
