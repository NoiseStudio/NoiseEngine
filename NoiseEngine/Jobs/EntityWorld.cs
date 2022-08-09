using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Jobs;

public partial class EntityWorld : IDisposable {

    private static uint nextId;

    private readonly ConcurrentList<EntitySystemBase> systems = new ConcurrentList<EntitySystemBase>();
    private readonly ConcurrentList<EntityGroup> groups = new ConcurrentList<EntityGroup>();
    private readonly ConcurrentDictionary<int, EntityGroup> idToGroup
        = new ConcurrentDictionary<int, EntityGroup>();
    private readonly ConcurrentDictionary<Entity, EntityGroup> entityToGroup =
        new ConcurrentDictionary<Entity, EntityGroup>();
    private readonly ConcurrentHashSet<WeakReference<EntityQueryBase>> queries =
        new ConcurrentHashSet<WeakReference<EntityQueryBase>>();
    private readonly ConcurrentDictionary<Type, ConcurrentList<EntitySystemBase>> typeToSystems =
        new ConcurrentDictionary<Type, ConcurrentList<EntitySystemBase>>();

    private ulong nextEntityId;
    private AtomicBool isDisposed;

    internal static EntityWorld Empty { get; } = null!;

    public uint Id { get; }
    public bool IsDestroyed => isDisposed;

    internal ComponentsStorage<Entity> ComponentsStorage { get; } = new ComponentsStorage<Entity>();

    public EntityWorld() {
        Id = Interlocked.Increment(ref nextId);
    }

    ~EntityWorld() {
        Dispose();
    }

    /// <summary>
    /// Checks if this entity world has T system
    /// </summary>
    /// <typeparam name="T">Entity system</typeparam>
    /// <returns>True when this entity world contains T system or false when not</returns>
    public bool HasSystem<T>(T system) where T : EntitySystemBase {
        return system.World == this;
    }

    /// <summary>
    /// Checks if this entity world has any T type system
    /// </summary>
    /// <typeparam name="T">Entity system</typeparam>
    /// <returns>True when this entity world contains T system or false when not</returns>
    public bool HasAnySystem<T>() where T : EntitySystemBase {
        return typeToSystems.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Returns first assigned T entity system
    /// </summary>
    /// <typeparam name="T">Entity system</typeparam>
    /// <returns><see cref="EntitySystemBase"/></returns>
    public T GetSystem<T>() where T : EntitySystemBase {
        return (T)typeToSystems[typeof(T)].First();
    }

    /// <summary>
    /// Returns T entity systems.
    /// </summary>
    /// <typeparam name="T"><see cref="EntitySystemBase"/> type.</typeparam>
    /// <returns><see cref="IReadOnlyList{T}"/> of <see cref="EntitySystemBase"/>.</returns>
    public IReadOnlyList<T> GetSystems<T>() where T : EntitySystemBase {
        return typeToSystems[typeof(T)].Cast<T>().ToArray();
    }

    /// <summary>
    /// Disposes this object and all systems.
    /// </summary>
    public void Dispose() {
        if (isDisposed.Exchange(true))
            return;

        foreach (EntitySystemBase system in systems)
            system.Dispose();

        systems.Clear();
        queries.Clear();
        typeToSystems.Clear();
        groups.Clear();
        idToGroup.Clear();
        entityToGroup.Clear();

        ComponentsStorage.Clear();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() {
        return $"{nameof(EntityWorld)}<{Id}>";
    }

    internal void AddSystem(EntitySystemBase system) {
        AssertIsNotDisposed();

        systems.Add(system);
        typeToSystems.GetOrAdd(system.GetType(), static _ => new ConcurrentList<EntitySystemBase>()).Add(system);
    }

    internal void RemoveSystem(EntitySystemBase system) {
        systems.Remove(system);
        if (typeToSystems.TryGetValue(system.GetType(), out ConcurrentList<EntitySystemBase>? list))
            list.Remove(system);
    }

    internal void AddQuery(WeakReference<EntityQueryBase> query) {
        AssertIsNotDisposed();
        queries.Add(query);
    }

    internal void RemoveQuery(WeakReference<EntityQueryBase> query) {
        queries.Remove(query);
    }

    internal void RegisterGroupsToQuery(EntityQueryBase query) {
        query.groups.Clear();
        foreach (EntityGroup group in groups)
            query.RegisterGroup(group);
    }

    internal EntityGroup GetGroupFromComponents(List<Type> components) {
        int hashCode = 0;
        foreach (Type component in components)
            hashCode ^= component.GetHashCode();

        return idToGroup.GetOrAdd(hashCode, _ => {
            EntityGroup group = new EntityGroup(hashCode, this, components);
            groups.Add(group);

            foreach (WeakReference<EntityQueryBase> queryReference in queries) {
                if (queryReference.TryGetTarget(out EntityQueryBase? query))
                    query.RegisterGroup(group);
            }

            return group;
        });
    }

    internal EntityGroup GetEntityGroup(Entity entity) {
        if (entityToGroup.TryGetValue(entity, out EntityGroup? group))
            return group;
        throw new InvalidOperationException($"{entity} was destroyed.");
    }

    internal void SetEntityGroup(Entity entity, EntityGroup group) {
        entityToGroup[entity] = group;
    }

    internal bool IsEntityDestroyed(Entity entity) {
        return !entityToGroup.ContainsKey(entity);
    }

    internal void DestroyEntity(Entity entity) {
        if (entityToGroup.TryRemove(entity, out EntityGroup? group))
            group.RemoveEntityWithDestroyComponents(entity);
    }

    private Entity NewEntityWorker() {
        AssertIsNotDisposed();
        return new Entity(Interlocked.Increment(ref nextEntityId));
    }

    private void AddNewEntityToGroup(Entity entity, List<Type> componentTypes) {
        EntityGroup group = GetGroupFromComponents(componentTypes);
        entityToGroup.GetOrAdd(entity, group);
        group.AddEntity(entity);
    }

    private void AssertIsNotDisposed() {
        if (IsDestroyed)
            throw new InvalidOperationException($"{ToString()} is disposed.");
    }

}
