using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NoiseStudio.JobsAg {
    internal class ConcurrentList<T> : IEnumerable<T>, IList<T> {

        private readonly List<T> list = new List<T>();
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public T this[int index] {
            get {
                locker.EnterReadLock();
                T item = list[index];
                locker.ExitReadLock();
                return item;
            }
            set {
                locker.EnterReadLock();
                list[index] = value;
                locker.ExitReadLock();
            }
        }

        public int Count {
            get {
                locker.EnterReadLock();
                int count = list.Count;
                locker.ExitReadLock();
                return count;
            }
        }

        public bool IsReadOnly => throw new NotImplementedException();

        public void Add(T item) {
            locker.EnterWriteLock();
            list.Add(item);
            locker.ExitWriteLock();
        }

        public void Clear() {
            locker.EnterWriteLock();
            list.Clear();
            locker.ExitWriteLock();
        }

        public bool Contains(T item) {
            locker.EnterReadLock();
            bool contains = list.Contains(item);
            locker.ExitReadLock();
            return contains;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            locker.EnterReadLock();
            list.CopyTo(array, arrayIndex);
            locker.ExitReadLock();
        }

        public IEnumerator<T> GetEnumerator() {
            locker.EnterReadLock();
            for (int i = 0; i < Count; i++) {
                yield return this[i];
            }
            locker.ExitReadLock();
        }

        public int IndexOf(T item) {
            locker.EnterReadLock();
            int index = list.IndexOf(item);
            locker.ExitReadLock();
            return index;
        }

        public void Insert(int index, T item) {
            locker.EnterWriteLock();
            list.Insert(index, item);
            locker.ExitWriteLock();
        }

        public bool Remove(T item) {
            locker.EnterWriteLock();
            bool removed = list.Remove(item);
            locker.ExitWriteLock();
            return removed;
        }

        public void RemoveAt(int index) {
            locker.EnterWriteLock();
            list.RemoveAt(index);
            locker.ExitWriteLock();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }
}
