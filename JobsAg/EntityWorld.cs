using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseStudio.JobsAg {
    public class EntityWorld {

        private static readonly object locker = new object();
        private static uint nextId = 0;

        private readonly Dictionary<int, EntityGroup> groups = new Dictionary<int, EntityGroup>();
        private readonly Dictionary<Entity, EntityGroup> entityToGroup = new Dictionary<Entity, EntityGroup>();

        private ulong nextEntityId = 0;

        public uint Id { get; }

        public EntityWorld() {
            lock (locker)
                Id = nextId++;
        }

        /// <summary>
        /// Creates new entity in this entity world
        /// </summary>
        /// <returns><see cref="Entity"/></returns>
        public Entity NewEntity() {
            Entity entity;
            lock (this)
                entity = new Entity(nextEntityId++);

            EntityGroup group = GetGroupFromComponents(new List<Type>());
            group.AddEntity(entity);

            lock (entityToGroup)
                entityToGroup.Add(entity, group);

            return entity;
        }

        internal EntityGroup GetGroupFromComponents(List<Type> components) {
            int hashCode = 0;
            components = components.OrderBy(t => t.GetHashCode()).ToList();
            for (int i = 0; i < components.Count; i++)
                hashCode ^= components[i].GetHashCode();

            EntityGroup? group;
            while (groups.TryGetValue(hashCode, out group) && !group.CompareSortedComponents(components))
                hashCode++;

            if (group == null) {
                lock (groups) {
                    group = new EntityGroup(hashCode, components);
                    groups.Add(hashCode, group);
                }
            }
            return group;
        }

        internal EntityGroup GetEntityGroup(Entity entity) {
            return entityToGroup[entity];
        }

        internal void SetEntityGroup(Entity entity, EntityGroup group) {
            entityToGroup[entity] = group;
        }

    }
}
