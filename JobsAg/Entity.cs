using System;
using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    public readonly struct Entity : IEquatable<Entity> {

        private readonly ulong id;

        internal Entity(ulong id) {
            this.id = id;
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation
        /// </summary>
        /// <returns>The string representation of the value of this instance</returns>
        public override string ToString() {
            return $"{nameof(Entity)}<{id.ToString("X")}>";
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode() {
            return id.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object
        /// </summary>
        /// <param name="obj">An object to compare to this instance</param>
        /// <returns>True if obj is an instance of Entity and equals the value of this instance or when not returns false</returns>
        public override bool Equals(object? obj) {
            return obj is Entity other && Equals(other);
        }

        /// <summary>
        /// Adds component to this entity
        /// </summary>
        /// <typeparam name="T">Struct inheriting from <see cref="IEntityComponent"/></typeparam>
        /// <param name="world">Entity world assigned to this entity</param>
        /// <param name="component">Component being added</param>
        public void Add<T>(EntityWorld world, T component) where T : struct, IEntityComponent {
            EntityGroup group = world.GetEntityGroup(this);
            Type type = component.GetType();

            if (group.HasComponent(type))
                throw new InvalidOperationException($"{ToString()} already has the {type.Name} component. Use the {"Set" /* TODO: replace with nameof */} method to replace this component.");

            List<Type> components = group.GetComponentsCopy();
            components.Add(type);

            group.RemoveEntity(this);
            group = world.GetGroupFromComponents(components);

            world.ComponentsStorage.AddComponent(this, component);

            world.SetEntityGroup(this, group);
            group.AddEntity(this);
        }

        /// <summary>
        /// Removes T component from this entity
        /// </summary>
        /// <typeparam name="T">Struct inheriting from <see cref="IEntityComponent"/></typeparam>
        /// <param name="world">Entity world assigned to this entity</param>
        public void Remove<T>(EntityWorld world) where T : struct, IEntityComponent {
            EntityGroup group = world.GetEntityGroup(this);
            Type type = typeof(T);

            if (!group.HasComponent(type))
                throw new InvalidOperationException($"{ToString()} does not have the {type.Name} component.");

            List<Type> components = group.GetComponentsCopy();
            components.Remove(type);

            group.RemoveEntity(this);
            group = world.GetGroupFromComponents(components);

            world.ComponentsStorage.RemoveComponent<T>(this);

            world.SetEntityGroup(this, group);
            group.AddEntity(this);
        }

        /// <summary>
        /// Checks if this entity has T component
        /// </summary>
        /// <typeparam name="T">Struct inheriting from <see cref="IEntityComponent"/></typeparam>
        /// <param name="world">Entity world assigned to this entity</param>
        /// <returns>Returns true when this entity contains T component and false when does not contains</returns>
        public bool Has<T>(EntityWorld world) where T : struct, IEntityComponent {
            return world.GetEntityGroup(this).HasComponent(typeof(T));
        }

        /// <summary>
        /// Returns T component assigned to this entity
        /// </summary>
        /// <typeparam name="T">Struct inheriting from <see cref="IEntityComponent"/></typeparam>
        /// <param name="world">Entity world assigned to this entity</param>
        /// <returns>T component assigned to this entity</returns>
        public T Get<T>(EntityWorld world) where T : struct, IEntityComponent {
            return world.ComponentsStorage.GetComponent<T>(this);
        }

        /// <summary>
        /// Replaces T component assigned to this entity
        /// </summary>
        /// <typeparam name="T">Struct inheriting from <see cref="IEntityComponent"/></typeparam>
        /// <param name="world">Entity world assigned to this entity</param>
        /// <param name="component">New component</param>
        public void Set<T>(EntityWorld world, T component) where T : struct, IEntityComponent {
            world.ComponentsStorage.SetComponent(this, component);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified other Entity
        /// </summary>
        /// <param name="other">An Entity to compare to this instance</param>
        /// <returns>True if other Entity is an instance of Entity and equals the value of this instance or when not returns false</returns>
        public bool Equals(Entity other) {
            return id == other.id;
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Entity"/></param>
        /// <param name="right"><see cref="Entity"/></param>
        /// <returns>True if left Entity is an instance of right Entity and equals the value of this instance or when not returns false</returns>
        public static bool operator ==(Entity left, Entity right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is not equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Entity"/></param>
        /// <param name="right"><see cref="Entity"/></param>
        /// <returns>False if left Entity is an instance of right Entity and equals the value of this instance or when not returns true</returns>
        public static bool operator !=(Entity left, Entity right) {
            return !(left == right);
        }

    }
}
