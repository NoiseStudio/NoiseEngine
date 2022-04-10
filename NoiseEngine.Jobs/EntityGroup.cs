using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs {
    internal class EntityGroup {

        internal readonly List<Entity> entities = new List<Entity>();

        private readonly int hashCode;
        private readonly IReadOnlyList<Type> components;
        private readonly HashSet<Type> componentsHashSet;
        private readonly ConcurrentQueue<Entity> entitiesToAdd = new ConcurrentQueue<Entity>();
        private readonly ConcurrentQueue<Entity> entitiesToRemove = new ConcurrentQueue<Entity>();
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        private bool clean;
        private uint ongoingWork;
        private uint readLock;
        private uint writeLock;
        private object? exclusiveWriteObject;

        public EntityWorld World { get; }

        internal IReadOnlyList<Type> ComponentTypes => components;

        public EntityGroup(int hashCode, EntityWorld world, List<Type> components) {
            this.hashCode = hashCode;
            this.components = components;
            World = world;

            componentsHashSet = new HashSet<Type>(components);
        }

        public override int GetHashCode() {
            return hashCode;
        }

        public void AddEntity(Entity entity) {
            entitiesToAdd.Enqueue(entity);
            DoWork();
        }

        public void RemoveEntity(Entity entity) {
            entitiesToRemove.Enqueue(entity);
            OrderWorkAndWait();

            for (int i = 0; i < entities.Count; i++) {
                if (entity == entities[i]) {
                    entities[i] = Entity.Empty;
                    break;
                }
            }
            clean = true;

            ReleaseWork();
        }

        public bool CompareSortedComponents(IReadOnlyList<Type> components) {
            if (this.components.Count != components.Count)
                return false;

            for (int i = 0; i < this.components.Count; i++) {
                if (this.components[i] != components[i])
                    return false;
            }
            return true;
        }

        public bool TryEnterReadLock(object? exclusiveWriteObject = null) {
            Interlocked.Increment(ref readLock);

            if (this.exclusiveWriteObject == null || this.exclusiveWriteObject == exclusiveWriteObject)
                return true;

            Interlocked.Decrement(ref readLock);
            return false;
        }

        public void ExitReadLock() {
            Interlocked.Decrement(ref readLock);
        }

        public bool TryEnterWriteLock(object exclusiveWriteObject) {
            if (readLock > 0)
                return false;

            object? obj = Interlocked.CompareExchange(ref this.exclusiveWriteObject, exclusiveWriteObject, null);
            if (obj != null && obj != exclusiveWriteObject)
                return false;

            if (readLock > 0) {
                Interlocked.CompareExchange(ref this.exclusiveWriteObject, null, exclusiveWriteObject);
                return false;
            }

            if (Interlocked.Increment(ref writeLock) == 1) {
                bool result = TryEnterWriteLock(exclusiveWriteObject);
                Interlocked.Decrement(ref writeLock);
                return result;
            }

            return true;
        }

        public void ExitWriteLock() {
            if (Interlocked.Decrement(ref writeLock) == 0)
                Interlocked.Exchange(ref exclusiveWriteObject, null);
        }

        public void OrderWork() {
            Interlocked.Increment(ref ongoingWork);
        }

        public void ReleaseWork() {
            if (Interlocked.Decrement(ref ongoingWork) == 0)
                DoWork();
        }

        public void OrderWorkAndWait() {
            OrderWork();
            manualResetEvent.WaitOne();
        }

        internal bool HasComponent(Type component) {
            return componentsHashSet.Contains(component);
        }

        internal void DestroyEntityComponents(Entity entity) {
            for (int i = 0; i < components.Count; i++)
                World.ComponentsStorage.RemoveComponent(components[i], entity);
        }

        private void DoWork() {
            manualResetEvent.Reset();

            if (ongoingWork > 0 || (!clean && entitiesToAdd.IsEmpty)) {
                manualResetEvent.Set();
                return;
            }

            if (clean) {
                clean = false;
                for (int i = 0; i < entities.Count; i++) {
                    if (entities[i] == Entity.Empty) {
                        entities.RemoveAt(i);
                        i--;
                    }
                }
            }

            while (entitiesToAdd.TryDequeue(out Entity entity))
                entities.Add(entity);
            while (entitiesToRemove.TryDequeue(out Entity entity))
                DestroyEntityComponents(entity);

            manualResetEvent.Set();
        }

    }
}
