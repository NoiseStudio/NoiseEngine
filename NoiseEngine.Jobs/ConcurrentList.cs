using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs {
    internal class ConcurrentList<T> : IEnumerable<T>, IList<T> {

        private readonly List<T> list = new List<T>();
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public T this[int index] {
            get {
                locker.EnterReadLock();
                T item;
                try {
                    item = list[index];
                } finally {
                    locker.ExitReadLock();
                }
                return item;
            }
            set {
                locker.EnterReadLock();
                try {
                    list[index] = value;
                } finally {
                    locker.ExitReadLock();
                }
            }
        }

        public int Count {
            get {
                locker.EnterReadLock();
                int count;
                try {
                    count = list.Count;
                } finally {
                    locker.ExitReadLock();
                }
                return count;
            }
        }

        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

        public void Add(T item) {
            locker.EnterWriteLock();
            try {
                list.Add(item);
            } finally {
                locker.ExitWriteLock();
            }
        }

        public void Clear() {
            locker.EnterWriteLock();
            try {
                list.Clear();
            } finally {
                locker.ExitWriteLock();
            }
        }

        public bool Contains(T item) {
            locker.EnterReadLock();
            bool contains;
            try {
                contains = list.Contains(item);
            } finally {
                locker.ExitReadLock();
            }
            return contains;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            locker.EnterReadLock();
            try {
                list.CopyTo(array, arrayIndex);
            } finally {
                locker.ExitReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator() {
            locker.EnterReadLock();
            try {
                for (int i = 0; i < Count; i++) {
                    yield return this[i];
                }
            } finally {
                locker.ExitReadLock();
            }
        }

        public int IndexOf(T item) {
            locker.EnterReadLock();
            int index;
            try {
                index = list.IndexOf(item);
            } finally {
                locker.ExitReadLock();
            }
            return index;
        }

        public void Insert(int index, T item) {
            locker.EnterWriteLock();
            try {
                list.Insert(index, item);
            } finally {
                locker.ExitWriteLock();
            }
        }

        public bool Remove(T item) {
            locker.EnterWriteLock();
            bool removed;
            try {
                removed = list.Remove(item);
            } finally {
                locker.ExitWriteLock();
            }
            return removed;
        }

        public void RemoveAt(int index) {
            locker.EnterWriteLock();
            try {
                list.RemoveAt(index);
            } finally {
                locker.ExitWriteLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

    }
}
