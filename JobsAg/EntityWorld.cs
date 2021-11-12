using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseStudio.JobsAg {
    public class EntityWorld {

        internal static EntityWorld Empty { get; private set; } = new EntityWorld();

        private static readonly object locker = new object();
        private static uint nextId = 0;

        internal readonly ComponentsStorage ComponentsStorage = new ComponentsStorage();

        private readonly List<EntitySystem> systems = new List<EntitySystem>();
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

        /// <summary>
        /// Creates and adds new T system to this world
        /// </summary>
        /// <typeparam name="T">Entity system</typeparam>
        /// <exception cref="InvalidOperationException">Entity world already contains T entity system</exception>
        public void AddSystem<T>() where T : EntitySystem, new() {
            if(HasSystem<T>())
                throw new InvalidOperationException($"Entity world already contains {typeof(T).FullName} entity system!");

            T system = new T();
            lock (systems)
                systems.Add(system);

            system.Init(this);
        }

        /// <summary>
        /// Removes T system from this world
        /// </summary>
        /// <typeparam name="T">Entity system</typeparam>
        /// <exception cref="InvalidOperationException">Entity world does not contains T entity system</exception>
        public void RemoveSystem<T>() where T : EntitySystem, new() {
            Type type = typeof(T);
            lock (systems) {
                for (int i = 0; i < systems.Count; i++) {
                    if (type == systems[i].GetType()) {
                        systems.RemoveAt(i);
                        return;
                    }
                }
            }
            throw new InvalidOperationException($"Entity world does not contains {type.FullName} entity system!");
        }

        /// <summary>
        /// Checks if this entity world has T system
        /// </summary>
        /// <typeparam name="T">Entity system</typeparam>
        /// <returns>True when this entity world contains T system or false when not</returns>
        public bool HasSystem<T>() where T : EntitySystem, new() {
            Type type = typeof(T);
            lock (systems) {
                for (int i = 0; i < systems.Count; i++) {
                    if (type == systems[i].GetType())
                        return true;
                }
            }
            return false;
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
