using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs;

internal class EntityGroup {

    internal const int PackageSize = 256;

    private const int DefaultCapacity = 4;

    private readonly int hashCode;
    private readonly IReadOnlyList<Type> components;
    private readonly HashSet<Type> componentsHashSet;
    private readonly List<ReaderWriterLockSlim> writeLock = new List<ReaderWriterLockSlim>();
    private readonly ManualResetEvent workResetEvent = new ManualResetEvent(true);
    private readonly ReaderWriterLockSlim entityLocker = new ReaderWriterLockSlim();
    private readonly ConcurrentQueue<Entity> entitiesToDestroyComponents = new ConcurrentQueue<Entity>();

    private Entity[] entities;
    private int count;
    private uint clean;
    private uint ongoingWork;

    public int PackageCount => (int)MathF.Ceiling(entities.Length / (float)PackageSize);
    public int EntityCount => count;

    public EntityWorld World { get; }

    internal IReadOnlyList<Type> ComponentTypes => components;
    internal IReadOnlyList<Entity> Entities => entities;

    public EntityGroup(int hashCode, EntityWorld world, List<Type> components) {
        this.hashCode = hashCode;
        this.components = components;
        World = world;

        componentsHashSet = new HashSet<Type>(components);
        entities = new Entity[DefaultCapacity];
    }

    public override int GetHashCode() {
        return hashCode;
    }

    public void AddEntity(Entity entity) {
        int index = Interlocked.Increment(ref count);
        EnsureCapacity(index--);

        int exceptedWriteLockCount = PackageCount;
        while (writeLock.Count < exceptedWriteLockCount)
            writeLock.Add(new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion));

        int maxIndex = Math.Min((index / PackageSize + 1) * PackageSize, entities.Length);
        for (int i = index; i < maxIndex; i++) {
            if (entities[i] != Entity.Empty)
                continue;

            entityLocker.EnterWriteLock();
            if (entities[i] == Entity.Empty) {
                entities[i] = entity;
                entityLocker.ExitWriteLock();
                return;
            }

            entityLocker.ExitWriteLock();
        }

        for (int i = index - 1; i >= 0; i--) {
            if (entities[i] != Entity.Empty)
                continue;

            entityLocker.EnterWriteLock();
            if (entities[i] == Entity.Empty) {
                entities[i] = entity;
                entityLocker.ExitWriteLock();
                return;
            }

            entityLocker.ExitWriteLock();
        }

        throw new NotImplementedException();
    }

    public void RemoveEntity(Entity entity) {
        Interlocked.Decrement(ref count);

        for (int i = 0; i < entities.Length; i++) {
            if (entities[i] != entity)
                continue;

            entityLocker.EnterReadLock();
            if (entities[i] == entity) {
                entities[i] = Entity.Empty;
                entityLocker.ExitReadLock();

                Interlocked.Increment(ref clean);
                DoWork();

                return;
            }

            entityLocker.ExitReadLock();
        }
    }

    public void RemoveEntityWithDestroyComponents(Entity entity) {
        entitiesToDestroyComponents.Enqueue(entity);
        RemoveEntity(entity);
    }

    public bool CompareSortedComponents(IReadOnlyList<Type> components) {
        if (this.components.Count != components.Count)
            return false;

        for (int i = 0; i < this.components.Count; i++) {
            if (this.components[i] != components[i])
                return false;
        }
        return true;
    }

    public bool TryEnterWriteLock(int startIndex) {
        return writeLock[startIndex / PackageSize].TryEnterWriteLock(0);
    }

    public void EnterWriteLock(int startIndex) {
        writeLock[startIndex / PackageSize].EnterWriteLock();
    }

    public void ExitWriteLock(int startIndex) {
        writeLock[startIndex / PackageSize].ExitWriteLock();
    }

    public void OrderWork() {
        Interlocked.Increment(ref ongoingWork);
    }

    public void ReleaseWork() {
        if (Interlocked.Decrement(ref ongoingWork) == 0)
            DoWork();
    }

    public void OrderWorkAndWait() {
        OrderWork();
        workResetEvent.WaitOne();
    }

    internal bool HasComponent(Type component) {
        return componentsHashSet.Contains(component);
    }

    internal void DestroyEntityComponents(Entity entity) {
        for (int i = 0; i < components.Count; i++)
            World.ComponentsStorage.RemoveComponent(components[i], entity);
    }

    private void DoWork() {
        while (entitiesToDestroyComponents.TryDequeue(out Entity entity))
            DestroyEntityComponents(entity);

        if (ongoingWork > 0 || clean <= PackageSize)
            return;

        workResetEvent.Reset();
        if (ongoingWork > 0 || clean <= PackageSize) {
            workResetEvent.Set();
            return;
        }

        if (clean >= PackageSize) {
            entityLocker.EnterWriteLock();

            Entity[] newEntities = new Entity[
                Math.Clamp(EntityCount * 2, DefaultCapacity, (EntityCount / PackageSize + 1) * PackageSize)];

            int i = 0;
            int j = 0;
            while (j < EntityCount) {
                Entity entity = entities[i++];
                if (entity != Entity.Empty)
                    newEntities[j++] = entity;
            }

            clean = 0;
            entities = newEntities;

            entityLocker.ExitWriteLock();
        }

        int exceptedWriteLockCount = PackageCount;
        while (writeLock.Count < exceptedWriteLockCount)
            writeLock.Add(new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion));
        while (writeLock.Count > exceptedWriteLockCount)
            writeLock.RemoveAt(writeLock.Count - 1);

        workResetEvent.Set();
    }

    private void EnsureCapacity(int count) {
        if (count <= entities.Length)
            return;

        entityLocker.EnterWriteLock();

        if (count > entities.Length) {
            int newCapacity = entities.Length == 0 ? DefaultCapacity : entities.Length * 2;
            if (newCapacity < count)
                newCapacity = count;
            else if (newCapacity - entities.Length > PackageSize)
                newCapacity = entities.Length + PackageSize;

            Array.Resize(ref entities, newCapacity);
        }

        entityLocker.ExitWriteLock();
    }

}
