using System;
using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    internal class EntityGroup {

        internal readonly List<Entity> entities = new List<Entity>();

        private readonly int hashCode;
        private readonly List<Type> components;
        private readonly HashSet<Type> componentsHashSet;

        public EntityGroup(int hashCode, List<Type> components) {
            this.hashCode = hashCode;
            this.components = components;

            componentsHashSet = new HashSet<Type>(components);
        }

        public override int GetHashCode() {
            return hashCode;
        }

        public void AddEntity(Entity entity) {
            lock (entities)
                entities.Add(entity);
        }

        public void RemoveEntity(Entity entity) {
            lock (entities)
                entities.Remove(entity);
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

    }
}
