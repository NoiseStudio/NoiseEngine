using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs {
    internal class EntityGroup {

        internal const int PackageSize = 256;

        private readonly int hashCode;
        private readonly IReadOnlyList<Type> components;
        private readonly HashSet<Type> componentsHashSet;
        private readonly List<Entity> entities = new List<Entity>();
        private readonly ConcurrentQueue<Entity> entitiesToAdd = new ConcurrentQueue<Entity>();
        private readonly ConcurrentQueue<Entity> entitiesToRemove = new ConcurrentQueue<Entity>();
        private readonly ManualResetEvent workResetEvent = new ManualResetEvent(false);
        private readonly List<ReaderWriterLockSlim> writeLock = new List<ReaderWriterLockSlim>();

        private bool clean;
        private uint ongoingWork;

        public EntityWorld World { get; }

        internal IReadOnlyList<Type> ComponentTypes => components;
        internal IReadOnlyList<Entity> Entities => entities;

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

        public void RemoveEntityWithDestroyComponents(Entity entity) {
            entitiesToRemove.Enqueue(entity);
            RemoveEntity(entity);
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

        public bool TryEnterWriteLock(int startIndex) {
            return writeLock[startIndex / PackageSize].TryEnterWriteLock(0);
        }

        public void EnterWriteLock(int startIndex) {
            writeLock[startIndex / PackageSize].EnterWriteLock();
        }

        public void ExitWriteLock(int startIndex) {
            writeLock[startIndex / PackageSize].ExitWriteLock();
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
            workResetEvent.WaitOne();
        }

        internal bool HasComponent(Type component) {
            return componentsHashSet.Contains(component);
        }

        internal void DestroyEntityComponents(Entity entity) {
            for (int i = 0; i < components.Count; i++)
                World.ComponentsStorage.RemoveComponent(components[i], entity);
        }

        private void DoWork() {
            workResetEvent.Reset();

            if (ongoingWork > 0 || (!clean && entitiesToAdd.IsEmpty)) {
                workResetEvent.Set();
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

            int exceptedWriteLockCount = (int)MathF.Ceiling(entities.Count / (float)PackageSize);
            while (writeLock.Count < exceptedWriteLockCount)
                writeLock.Add(new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion));
            while (writeLock.Count > exceptedWriteLockCount)
                writeLock.RemoveAt(writeLock.Count - 1);

            workResetEvent.Set();
        }

    }
}
