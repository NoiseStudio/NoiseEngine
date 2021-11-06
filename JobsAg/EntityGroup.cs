using System;
using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    internal class EntityGroup {

        internal readonly List<Entity> entities = new List<Entity>();

        private readonly int hashCode;
        private readonly Type[] components;

        public EntityGroup(int hashCode, Type[] components) {
            this.hashCode = hashCode;
            this.components = components;
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
            if (this.components.Length != components.Count)
                return false;

            for (int i = 0; i < this.components.Length; i++) {
                if (this.components[i] != components[i])
                    return false;
            }
            return true;
        }

    }
}
