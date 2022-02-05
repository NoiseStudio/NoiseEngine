using NoiseEngine.Collections.Concurrent;
using Xunit;

namespace NoiseEngine.Collections.Tests.Concurrent {
    public class ConcurrentListTest {

        [Fact]
        public void Indexor() {
            ConcurrentList<int> list = new ConcurrentList<int>();
            list.Add(1);

            Assert.Equal(1, list[0]);
            list[0] = 2;
            Assert.Equal(2, list[0]);
        }

        [Fact]
        public void Add() {
            ConcurrentList<int> list = new ConcurrentList<int>();
            list.Add(1);
        }

        [Fact]
        public void Clear() {
            ConcurrentList<int> list = new ConcurrentList<int>();
            list.Add(1);
            list.Clear();

            Assert.Empty(list);
        }

        [Fact]
        public void Contains() {
            ConcurrentList<int> list = new ConcurrentList<int>();
            list.Add(1);

            Assert.Contains(1, list);
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
        public void IndexOf() {
            ConcurrentList<bool> list = new ConcurrentList<bool>();
            list.Add(true);
            list.Add(false);

            Assert.Equal(1, list.IndexOf(false));
        }

        [Fact]
        public void Insert() {
            ConcurrentList<bool> list = new ConcurrentList<bool>();
            list.Add(true);
            list.Add(true);

            Assert.True(list[0]);
            Assert.True(list[1]);

            list.Insert(1, false);

            Assert.True(list[0]);
            Assert.False(list[1]);
            Assert.True(list[2]);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void Remove() {
            ConcurrentList<bool> list = new ConcurrentList<bool>();
            list.Add(true);
            list.Add(true);

            Assert.True(list[0]);
            Assert.True(list[1]);

            list.Remove(true);

            Assert.True(list[0]);
            Assert.Single(list);
        }

        [Fact]
        public void RemoveAt() {
            ConcurrentList<int> list = new ConcurrentList<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            Assert.Equal(1, list[0]);
            Assert.Equal(2, list[1]);

            list.RemoveAt(1);

            Assert.Equal(1, list[0]);
            Assert.Equal(3, list[1]);
        }

        [Fact]
        public void WriteWork() {
            ConcurrentList<int> list = new ConcurrentList<int>();
            list.WriteWork(() => {
                list.Add(1);
                list.Add(35);
            });

            Assert.Equal(1, list[0]);
            Assert.Equal(35, list[1]);
        }

        [Fact]
        public void ToArray() {
            ConcurrentList<int> list = new ConcurrentList<int>();
            int[] items = new int[] { 1324, 333 };

            foreach (int item in items)
                list.Add(item);

            Assert.Equal(items, list.ToArray());
        }

    }
}
