using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseStudio.JobsAg {
    public class EntityWorld {

        private static readonly object locker = new object();
        private static uint nextId = 0;

        private readonly Dictionary<int, EntityGroup> groups = new Dictionary<int, EntityGroup>();
        private readonly ConcurrentDictionary<Entity, EntityGroup> entityToGroup = new ConcurrentDictionary<Entity, EntityGroup>();

        private ulong nextEntityId = 0;

        public uint Id { get; init; }

        public EntityWorld() {
            lock (locker)
                Id = nextId++;
        }

        /// <summary>
        /// Creates new entity in this entity world
        /// </summary>
        /// <returns><see cref="Entity"/></returns>
        public Entity NewEntity() {
            lock (this)
                return new Entity(nextEntityId++);
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
                    group = new EntityGroup(hashCode, components.ToArray());
                    groups.Add(hashCode, group);
                }
            }
            return group;
        }

    }
}
