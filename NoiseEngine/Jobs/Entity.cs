namespace NoiseEngine.Jobs;

public readonly partial struct Entity : IEquatable<Entity> {

    private readonly ulong id;

    public static Entity Empty => new Entity(0);

    /// <summary>
    /// Do not use default constructor for this type, always throws <see cref="InvalidOperationException"/>.
    /// Use EntityWorld.NewEntity method instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Always throws <see cref="InvalidOperationException"/>.
    /// </exception>
    [Obsolete($"Do not use default constructor for this type. Use {nameof(EntityWorld.NewEntity)} method instead.", true)]
    public Entity() {
        throw new InvalidOperationException($"Do not use default constructor for this type. Use {nameof(EntityWorld.NewEntity)} method instead.");
    }

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
            throw new InvalidOperationException($"{ToString()} already has the {type.Name} component. Use the {nameof(Set)} method to replace this component.");

        world.ComponentsStorage.AddComponent(this, component);

        List<Type> components = new List<Type>(group.ComponentTypes);
        components.Add(type);

        group.RemoveEntity(this);
        group = world.GetGroupFromComponents(components);

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

        List<Type> components = new List<Type>(group.ComponentTypes);
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
    /// Destroys this entity
    /// </summary>
    /// <param name="world">Entity world assigned to this entity</param>
    public void Destroy(EntityWorld world) {
        world.DestroyEntity(this);
    }

    /// <summary>
    /// Checks if this entity was destroyed
    /// </summary>
    /// <param name="world">Entity world assigned to this entity</param>
    /// <returns>True when this entity was destroyed or false when not</returns>
    public bool IsDestroyed(EntityWorld world) {
        return world.IsEntityDestroyed(this);
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
