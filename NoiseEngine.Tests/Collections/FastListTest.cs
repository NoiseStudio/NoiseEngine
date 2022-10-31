using NoiseEngine.Collections;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Tests.Collections;

public class FastListTest {

    private int[] TestArray => new int[] { 1, 10, 11, 100, 101, 111, 1000 };

    [Fact]
    public void Indexer() {
        FastList<int> list = new FastList<int>(TestArray);

        Assert.Equal(TestArray[0], list[0]);
        list[0] = 2;
        Assert.Equal(2, list[0]);
    }

    [Fact]
    public void Add() {
        FastList<int> list = new FastList<int>();

        foreach (int item in TestArray)
            list.Add(item);

        Assert.Equal(TestArray, list);
    }

    [Fact]
    public void AddRangeSpan() {
        FastList<int> list = new FastList<int>();
        list.AddRange(TestArray.AsSpan());

        Assert.Equal(TestArray, list);

        Span<int> a = stackalloc int[5];
        list.AddRange(a);
    }

    [Fact]
    public void Clear() {
        FastList<int> list = new FastList<int>(TestArray);
        list.Clear();

        Assert.Empty(list);
    }

    [Fact]
    public void Contains() {
        FastList<int> list = new FastList<int>(TestArray);

        Assert.Contains(TestArray[3], list);
    }

    [Fact]
    public void CopyToArray() {
        FastList<int> list = new FastList<int>(TestArray.Length * 2);
        list.AddRange(TestArray);
        int[] data = new int[TestArray.Length];

        list.CopyTo(data, 0);
        Assert.Equal(data, list);
    }

    [Fact]
    public void CopyToFastList() {
        FastList<int> list = new FastList<int>(TestArray);
        FastList<int> list2 = new FastList<int>();

        list.CopyTo(list2, 0);

        Assert.Equal(list, list2);
    }

    [Fact]
    public void IndexOf() {
        FastList<int> list = new FastList<int>(TestArray);

        Assert.Equal(5, list.IndexOf(TestArray[5]));
        Assert.Equal(-1, list.IndexOf(-1));
    }

    [Fact]
    public void Insert() {
        List<int> list2 = new List<int>(TestArray);
        FastList<int> list = new FastList<int>(TestArray);

        list2.Insert(4, 2);
        list.Insert(4, 2);

        Assert.Equal(list2, list);
    }

    [Fact]
    public void InsertRangeSpan() {
        List<int> list2 = new List<int>(TestArray);
        FastList<int> list = new FastList<int>(TestArray);

        list2.InsertRange(4, TestArray);
        list.InsertRange(4, TestArray.AsSpan());

        Assert.Equal(list2, list);
    }

    [Fact]
    public void Remove() {
        List<int> list2 = new List<int>(TestArray);
        FastList<int> list = new FastList<int>(TestArray);

        list2.Remove(TestArray[4]);
        Assert.True(list.Remove(TestArray[4]));

        Assert.Equal(list2, list);
    }

    [Fact]
    public void RemoveLastElement() {
        List<int> list2 = new List<int>(TestArray);
        FastList<int> list = new FastList<int>(TestArray);

        list2.Remove(TestArray[TestArray.Length - 1]);
        Assert.True(list.Remove(TestArray[TestArray.Length - 1]));

        Assert.Equal(list2, list);
    }

    [Fact]
    public void RemoveLast() {
        FastList<int> list = new FastList<int>(TestArray);

        Assert.Equal(TestArray.Length, list.Count);
        list.RemoveLast(2);
        Assert.Equal(TestArray.AsSpan(0, TestArray.Length - 2).ToArray(), list);
    }

    [Fact]
    public void ToArray() {
        List<int> list2 = new List<int>(TestArray);
        FastList<int> list = new FastList<int>(TestArray);

        list2.AddRange(TestArray);
        list.AddRange(TestArray);

        list2.Add(3);
        list.Add(3);

        Assert.Equal(list2, list.ToArray());
    }

}
