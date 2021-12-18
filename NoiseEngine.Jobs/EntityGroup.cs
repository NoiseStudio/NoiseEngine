using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace NoiseEngine.Jobs {
    internal class EntityGroup {

        internal readonly List<Entity> entities = new List<Entity>();

        private readonly int hashCode;
        private readonly ReadOnlyCollection<Type> components;
        private readonly HashSet<Type> componentsHashSet;
        private readonly ConcurrentQueue<Entity> entitiesToAdd = new ConcurrentQueue<Entity>();
        private readonly ConcurrentQueue<Entity> entitiesToRemove = new ConcurrentQueue<Entity>();

        private readonly object locker = new object();
        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(false);

        private bool clean = false;
        private int ongoingWork = 0;

        public EntityWorld World { get; }

        internal ReadOnlyCollection<Type> ComponentTypes => components;

        public EntityGroup(int hashCode, EntityWorld world, List<Type> components) {
            this.hashCode = hashCode;
            this.components = new ReadOnlyCollection<Type>(components);
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
            Wait();

            for (int i = 0; i < entities.Count; i++) {
                if (entity == entities[i]) {
                    entities[i] = Entity.Empty;
                    break;
                }
            }
            clean = true;

            ReleaseWork();
        }

        public bool CompareSortedComponents(List<Type> components) {
            if (this.components.Count != components.Count)
                return false;

            for (int i = 0; i < this.components.Count; i++) {
                if (this.components[i] != components[i])
                    return false;
            }
            return true;
        }

        public void OrderWork() {
            lock (locker)
                ongoingWork++;
        }

        public void ReleaseWork() {
            lock (locker) {
                ongoingWork--;
                DoWork();
            }
        }

        public void Wait() {
            lock (locker) {
                OrderWork();
                manualResetEvent.WaitOne();
            }
        }

        internal List<Type> GetComponentsCopy() {
            return new List<Type>(components);
        }

        internal bool HasComponent(Type component) {
            return componentsHashSet.Contains(component);
        }

        internal void DestroyEntityComponents(Entity entity) {
            for (int i = 0; i < components.Count; i++)
                World.ComponentsStorage.RemoveComponent(components[i], entity);
        }

        private void DoWork() {
            lock (locker) {
                if (ongoingWork > 0 || (!clean && entitiesToAdd.IsEmpty))
                    return;
                manualResetEvent.Reset();

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
}
