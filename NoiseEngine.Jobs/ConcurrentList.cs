﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs {
    internal class ConcurrentList<T> : IEnumerable<T>, IList<T>, IList {

        private readonly List<T> list = new List<T>();
        private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        public T this[int index] {
            get {
                locker.EnterUpgradeableReadLock();
                T item;
                try {
                    item = list[index];
                } finally {
                    locker.ExitUpgradeableReadLock();
                }
                return item;
            }
            set {
                locker.EnterUpgradeableReadLock();
                try {
                    list[index] = value;
                } finally {
                    locker.ExitUpgradeableReadLock();
                }
            }
        }

        public int Count {
            get {
                locker.EnterUpgradeableReadLock();
                int count;
                try {
                    count = list.Count;
                } finally {
                    locker.ExitUpgradeableReadLock();
                }
                return count;
            }
        }

        object? IList.this[int index] {
            get {
                return this[index];
            }
            set {
                this[index] = (T)value!;
            }
        }

        public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;
        public bool IsSynchronized => ((IList)list).IsSynchronized;
        public object SyncRoot => ((IList)list).SyncRoot;
        bool IList.IsFixedSize => ((IList)list).IsFixedSize;

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
            locker.EnterUpgradeableReadLock();
            bool contains;
            try {
                contains = list.Contains(item);
            } finally {
                locker.ExitUpgradeableReadLock();
            }
            return contains;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            locker.EnterUpgradeableReadLock();
            try {
                list.CopyTo(array, arrayIndex);
            } finally {
                locker.ExitUpgradeableReadLock();
            }
        }

        public IEnumerator<T> GetEnumerator() {
            locker.EnterUpgradeableReadLock();
            try {
                for (int i = 0; i < Count; i++) {
                    yield return this[i];
                }
            } finally {
                locker.ExitUpgradeableReadLock();
            }
        }

        public int IndexOf(T item) {
            locker.EnterUpgradeableReadLock();
            int index;
            try {
                index = list.IndexOf(item);
            } finally {
                locker.ExitUpgradeableReadLock();
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

        public void WriteWork(Action action) {
            locker.EnterWriteLock();
            try {
                action.Invoke();
            } finally {
                locker.ExitWriteLock();
            }
        }

        public T[] ToArray() {
            locker.EnterUpgradeableReadLock();
            T[] result;
            try {
                result = list.ToArray();
            } finally {
                locker.ExitUpgradeableReadLock();
            }
            return result;
        }

        public List<T> ToList() {
            locker.EnterUpgradeableReadLock();
            List<T> result;
            try {
                result = new List<T>(list);
            } finally {
                locker.ExitUpgradeableReadLock();
            }
            return result;
        }

        public void CopyTo(Array array, int index) {
            locker.EnterUpgradeableReadLock();
            try {
                ((IList)list).CopyTo(array, index);
            } finally {
                locker.ExitUpgradeableReadLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        int IList.Add(object? value) {
            if (!IsCompatibleObject(value))
                return -1;

            int result = -1;
            locker.EnterWriteLock();
            try {
                list.Add((T)value!);
                result = list.Count - 1;
            } finally {
                locker.ExitWriteLock();
            }
            return result;
        }

        bool IList.Contains(object? value) {
            if (IsCompatibleObject(value))
                return Contains((T)value!);
            return false;
        }

        int IList.IndexOf(object? value) {
            if (IsCompatibleObject(value))
                return IndexOf((T)value!);
            return -1;
        }

        void IList.Insert(int index, object? value) {
            if (IsCompatibleObject(value))
                Insert(index, (T)value!);
        }

        void IList.Remove(object? value) {
            if (IsCompatibleObject(value))
                Remove((T)value!);
        }

        private static bool IsCompatibleObject(object? value) {
            return (value is T) || (value == null && default(T) == null);
        }

    }
}