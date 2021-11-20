using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace NoiseStudio.JobsAg {
    internal class EntityGroup {

        internal readonly List<Entity> entities = new List<Entity>();

        private readonly int hashCode;
        private readonly List<Type> components;
        private readonly HashSet<Type> componentsHashSet;
        private readonly ConcurrentQueue<Entity> entitiesToAdd = new ConcurrentQueue<Entity>();

        private readonly object locker = new object();
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private bool clean = false;
        private int ongoingWork = 0;
        private bool isWorking = false;
        private long workEndTime = 0;

        public EntityGroup(int hashCode, List<Type> components) {
            this.hashCode = hashCode;
            this.components = components;

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
            OrderWork();
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
            if (isWorking && Stopwatch.GetTimestamp() - workEndTime > 32)
                autoResetEvent.WaitOne();
        }

        internal List<Type> GetComponentsCopy() {
            return new List<Type>(components);
        }

        internal bool HasComponent(Type component) {
            return componentsHashSet.Contains(component);
        }

        internal void DestroyEntityComponents(EntityWorld world, Entity entity) {
            for (int i = 0; i < components.Count; i++)
                world.ComponentsStorage.RemoveComponent(components[i], entity);
        }

        private void DoWork() {
            lock (locker) {
                if (ongoingWork > 0 || (!clean && entitiesToAdd.IsEmpty) || isWorking)
                    return;
                isWorking = true;

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

                workEndTime = Stopwatch.GetTimestamp();
                isWorking = false;
                autoResetEvent.Set();
            }
        }

    }
}
