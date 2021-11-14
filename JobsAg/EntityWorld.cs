using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseStudio.JobsAg {
    public class EntityWorld {

        private static readonly object locker = new object();
        private static uint nextId = 0;


        private readonly List<EntitySystemBase> systems = new List<EntitySystemBase>();
        private readonly List<EntitySystemBase> disabledSystems = new List<EntitySystemBase>();
        private readonly List<EntityGroup> groups = new List<EntityGroup>();
        private readonly Dictionary<int, EntityGroup> idToGroup = new Dictionary<int, EntityGroup>();
        private readonly Dictionary<Entity, EntityGroup> entityToGroup = new Dictionary<Entity, EntityGroup>();

        private ulong nextEntityId = 0;

        internal static EntityWorld Empty { get; } = new EntityWorld();

        internal ComponentsStorage ComponentsStorage { get; } = new ComponentsStorage();

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
            Entity entity = NewEntityWorker(new List<Type>());

            return entity;
        }

        /// <summary>
        /// Creates new entity in this entity world
        /// </summary>
        /// <typeparam name="T">Struct inheriting from <see cref="IEntityComponent"/></typeparam>
        /// <param name="component">Component being added</param>
        /// <returns><see cref="Entity"/></returns>
        public Entity NewEntity<T>(T component) where T : struct, IEntityComponent {
            Entity entity = NewEntityWorker(new List<Type>() {
                typeof(T)
            });

            ComponentsStorage.AddComponent(entity, component);

            return entity;
        }

        /// <summary>
        /// Creates and adds new T system to this world
        /// </summary>
        /// <typeparam name="T">Entity system</typeparam>
        /// <exception cref="InvalidOperationException">Entity world already contains T entity system</exception>
        public void AddSystem<T>() where T : EntitySystemBase, new() {
            if(HasSystem<T>())
                throw new InvalidOperationException($"Entity world already contains {typeof(T).FullName} entity system!");

            T system = new T();
            lock (systems)
                systems.Add(system);

            lock (groups) {
                for (int i = 0; i < groups.Count; i++)
                    system.RegisterGroup(groups[i]);
            }

            system.InternalInitialize(this);
            system.InternalStart();
        }

        /// <summary>
        /// Removes T system from this world
        /// </summary>
        /// <typeparam name="T">Entity system</typeparam>
        /// <exception cref="InvalidOperationException">Entity world does not contains T entity system</exception>
        public void RemoveSystem<T>() where T : EntitySystemBase, new() {
            Type type = typeof(T);
            lock (systems) {
                for (int i = 0; i < systems.Count; i++) {
                    EntitySystemBase system = systems[i];
                    if (type == system.GetType()) {
                        systems.RemoveAt(i);

                        system.InternalStop();
                        system.InternalTerminate();
                        return;
                    }
                }
            }
            lock (disabledSystems) {
                for (int i = 0; i < disabledSystems.Count; i++) {
                    EntitySystemBase system = disabledSystems[i];
                    if (type == disabledSystems[i].GetType()) {
                        disabledSystems.RemoveAt(i);

                        system.InternalTerminate();
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
        public bool HasSystem<T>() where T : EntitySystemBase, new() {
            Type type = typeof(T);
            lock (systems) {
                for (int i = 0; i < systems.Count; i++) {
                    if (type == systems[i].GetType())
                        return true;
                }
            }
            lock (disabledSystems) {
                for (int i = 0; i < disabledSystems.Count; i++) {
                    if (type == disabledSystems[i].GetType())
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Enabling T entity system
        /// </summary>
        /// <typeparam name="T">Entity system</typeparam>
        public void EnableSystem<T>() where T : EntitySystemBase, new() {
            Type type = typeof(T);
            lock (disabledSystems) {
                for (int i = 0; i < disabledSystems.Count; i++) {
                    EntitySystemBase system = disabledSystems[i];
                    if (type == system.GetType()) {
                        disabledSystems.RemoveAt(i);
                        lock (systems)
                            systems.Add(system);

                        system.InternalStart();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Disabling T entity system
        /// </summary>
        /// <typeparam name="T">Entity system</typeparam>
        public void DisableSystem<T>() where T : EntitySystemBase, new() {
            Type type = typeof(T);
            lock (systems) {
                for (int i = 0; i < systems.Count; i++) {
                    EntitySystemBase system = systems[i];
                    if (type == system.GetType()) {
                        systems.RemoveAt(i);
                        lock (disabledSystems)
                            disabledSystems.Add(system);

                        system.InternalStop();
                        return;
                    }
                }
            }
        }

        internal EntityGroup GetGroupFromComponents(List<Type> components) {
            int hashCode = 0;
            components = components.OrderBy(t => t.GetHashCode()).ToList();
            for (int i = 0; i < components.Count; i++)
                hashCode ^= components[i].GetHashCode();

            EntityGroup? group;
            while (idToGroup.TryGetValue(hashCode, out group) && !group.CompareSortedComponents(components))
                hashCode++;

            if (group == null) {
                lock (idToGroup) {
                    group = new EntityGroup(hashCode, components);
                    idToGroup.Add(hashCode, group);
                }
                lock (group)
                    groups.Add(group);

                lock (systems) {
                    foreach (EntitySystem system in systems)
                        system.RegisterGroup(group);
                }
                lock (disabledSystems) {
                    foreach (EntitySystem system in disabledSystems)
                        system.RegisterGroup(group);
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

        private Entity NewEntityWorker(List<Type> componentTypes) {
            Entity entity;
            lock (this)
                entity = new Entity(nextEntityId++);

            EntityGroup group = GetGroupFromComponents(componentTypes);
            group.AddEntity(entity);

            lock (entityToGroup)
                entityToGroup.Add(entity, group);

            return entity;
        }

    }
}
