using NoiseEngine.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs.Commands;

internal class SystemCommandsExecutor {

    private readonly FastList<SystemCommand> commands;
    private readonly Dictionary<Type, (IComponent? value, int affectiveHashCode)> components =
        new Dictionary<Type, (IComponent? value, int affectiveHashCode)>();

    private SystemCommandsInner? inner;
    private int index;
    private EntityCommandsInner? entityCommands;
    private bool writeAccess;
    private List<(Type, IComponent, int)>? changed;

    public SystemCommandsExecutor(FastList<SystemCommand> commands) {
        this.commands = commands;
    }

    public void Invoke() {
        while (index < commands.Count) {
            SystemCommand command = commands[index];
            switch (command.Type) {
                case SystemCommandType.GetEntity:
                    EntityCommandsInner entityCommandsTemp = (EntityCommandsInner)command.Value!;
                    if (entityCommands?.Entity != entityCommandsTemp.Entity) {
                        ProcessEntity();
                        components.Clear();
                        writeAccess = false;
                    }
                    entityCommands = entityCommandsTemp;
                    break;
                case SystemCommandType.EntityDespawn:
                    DespawnEntity();
                    while (++index < commands.Count && commands[index].Type != SystemCommandType.GetEntity)
                        continue;
                    index--;
                    break;
                case SystemCommandType.EntityInsert:
                    writeAccess = true;
                    (IComponent component, int affectiveHashCode) value = ((IComponent, int))command.Value!;
                    Type type = value.component.GetType();
                    components[type] = value;
                    break;
                case SystemCommandType.EntityRemove:
                    Type typeB = (Type)command.Value!;
                    writeAccess |= entityCommands!.Entity.Contains(typeB);
                    components[typeB] = (null, 0);
                    break;
                default:
                    throw new UnreachableException();
            }
            index++;
        }

        ProcessEntity();
    }

    private void ProcessEntity() {
        if (entityCommands is null || components.Count == 0)
            return;

        Entity entity = entityCommands.Entity;
        EntityLockerHeld held;
        if (entityCommands.ConditionalsCount > 0) {
            (Entity, bool)[] entities = new (Entity, bool)[entityCommands.ConditionalsCount + 1];
            entities[0] = (entity, writeAccess);

            for (int i = 0; i < entityCommands.ConditionalsCount; i++)
                entities[i + 1] = (entityCommands.Conditionals[i].Entity, false);

            if (!EntityLocker.TryLockEntities(entities, out held))
                return;
        } else if (!EntityLocker.TryLockEntity(entity, writeAccess, out held)) {
            return;
        }

        if (!writeAccess) {
            foreach ((Type type, (IComponent? value, _)) in components) {
                if ((value is null && entityCommands.Entity.Contains(type)) || !entityCommands.Entity.Contains(type)) {
                    held.Dispose();
                    writeAccess = true;
                    ProcessEntity();
                    return;
                }
            }
        }

        if (entityCommands.ConditionalsCount == 0) {
            int hashCode = 0;
            foreach ((Type type, int affectiveHashCode) in components.Where(
                x => x.Value.value is not null
            ).Select(x => (x.Key, x.Value.affectiveHashCode)).UnionBy(
                entity.chunk!.Archetype.ComponentTypes.Select(x => (x.type, x.affectiveHashCode)).Where(
                    x => !components.TryGetValue(x.type, out (IComponent? value, int affectiveHashCode) o) ||
                        o.value is not null
                ), x => x.Item1)
            ) {
                hashCode ^= unchecked(type.GetHashCode() + affectiveHashCode * 16777619);
            }

            EntityWorld world = entity.chunk!.Archetype.World;
            if (!world.TryGetArchetype(hashCode, out Archetype? newArchetype)) {
                newArchetype = world.CreateArchetype(hashCode, components.Where(x => x.Value.value is not null)
                    .Select(x => (x.Key, x.Value.affectiveHashCode))
                    .UnionBy(entity.chunk!.Archetype.ComponentTypes.Where(
                        x => !components.TryGetValue(
                            x.type, out (IComponent? value, int affectiveHashCode) o
                        ) || o.value is not null
                    ).Select(x => (x.type, x.affectiveHashCode)), x => x.Item1).ToArray()
                );
            }

            if (newArchetype != entity.chunk.Archetype)
                ChangeArchetype(newArchetype, held);
            else
                UpdateRecord(held);
            return;
        }

        held.Dispose();
    }

    private void DespawnEntity() {
        if (entityCommands is null)
            return;

        Entity entity = entityCommands!.Entity;
        if (!EntityLocker.TryLockEntity(entity, true, out EntityLockerHeld held))
            return;

        ArchetypeChunk chunk = entity.chunk!;
        entity.chunk = null;
        nint index = entity.index;
        entity.index = 0;

        unsafe {
            fixed (byte* dp = chunk.StorageData) {
                byte* di = dp + index;

                // Clear old data.
                new Span<byte>(di, (int)chunk.Archetype.RecordSize).Clear();
            }
        }

        held.Dispose();
        chunk.Archetype.ReleaseRecord(chunk, index);
    }

    private void ChangeArchetype(Archetype newArchetype, EntityLockerHeld held) {
        Entity entity = entityCommands!.Entity;
        ArchetypeChunk oldChunk = entity.chunk!;
        nint oldIndex = entity.index;
        (ArchetypeChunk newChunk, nint newIndex) = newArchetype.TakeRecord();

        unsafe {
            fixed (byte* dp = newChunk.StorageData) {
                byte* di = dp + newIndex;

                Debug.Assert(Unsafe.Read<EntityInternalComponent>(di).Entity is null);

                fixed (byte* sp = oldChunk.StorageData) {
                    byte* si = sp + oldIndex;

                    foreach ((Type type, int size, _) in newChunk.Archetype.ComponentTypes) {
                        (IComponent? value, int affectiveHashCode) component;
                        if (oldChunk.Offsets.TryGetValue(type, out nint oldOffset)) {
                            // Copy old component.
                            if (!components.TryGetValue(type, out component)) {
                                Buffer.MemoryCopy(si + oldOffset, di + newChunk.Offsets[type], size, size);
                                continue;
                            }

                            // Append changed observers.
                            changed ??= new List<(Type, IComponent, int)>();
                            IComponent old = oldChunk.ReadComponentBoxed(type, size, (nint)si + oldChunk.Offsets[type]);
                            changed.Add((type, old, size));
                        } else if (!components.TryGetValue(type, out component)) {
                            // Skip if appended default.
                            Debug.Assert(newArchetype.Offsets.ContainsKey(type));
                            continue;
                        }

                        // Copy new component.
                        nint componentPointer = (nint)di + newChunk.Offsets[type];
                        ComponentMemoryCopy(ref component.value!, type, componentPointer, size);
                    }

                    // Set internal component.
                    Unsafe.AsRef<EntityInternalComponent>(di) = new EntityInternalComponent(entity);

                    // Clear old data.
                    Debug.Assert(Unsafe.Read<EntityInternalComponent>(si).Entity == entity);
                    new Span<byte>(si, (int)oldChunk.Archetype.RecordSize).Clear();

                    Debug.Assert(Unsafe.Read<EntityInternalComponent>(si).Entity is null);
                    Debug.Assert(Unsafe.Read<EntityInternalComponent>(di).Entity == entity);

                    oldChunk.Archetype.ReleaseRecord(oldChunk, oldIndex);

                    // Notify changed observers.
                    if (changed is not null) {
                        foreach ((Type type, IComponent component, int size) in changed) {
                            IComponent c = component;
                            NotifyChangeObserver(ref c, entity, newChunk, type, (nint)di, size);
                        }
                        changed.Clear();
                    }
                }
            }
        }

        entity.chunk = newChunk;
        entity.index = newIndex;

        held.Dispose();
        newArchetype.InitializeRecord();
    }

    private void UpdateRecord(EntityLockerHeld held) {
        Entity entity = entityCommands!.Entity;
        ArchetypeChunk chunk = entity.chunk!;

        unsafe {
            fixed (byte* dp = chunk.StorageData) {
                byte* di = dp + entity.index;

                // Update components.
                foreach ((Type type, (IComponent? value, int affectiveHashCode)) in components) {
                    if (value is null)
                        continue;

                    (nint offset, int size) = chunk.ExtendedInformation[type];
                    nint componentPointer = (nint)di + offset;
                    IComponent old = chunk.ReadComponentBoxed(type, size, componentPointer);
                    IComponent component = value;
                    ComponentMemoryCopy(ref component, type, componentPointer, size);
                    NotifyChangeObserver(ref old, entity, chunk, type, (nint)di, size);
                }
            }
        }

        held.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void ComponentMemoryCopy(ref IComponent component, Type type, nint componentPointer, int size) {
        if (size == sizeof(nint) && !type.IsValueType) {
            fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref component))
                Buffer.MemoryCopy(vp, (void*)componentPointer, size, size);
            return;
        }

        fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref component)) {
            Buffer.MemoryCopy(
                (void*)(Unsafe.Read<IntPtr>(vp) + sizeof(nint)),
                (void*)componentPointer, size, size
            );
        }
    }

    private unsafe void NotifyChangeObserver(
        ref IComponent oldValue, Entity entity, ArchetypeChunk newChunk, Type type, nint entityPointer, int size
    ) {
        inner ??= new SystemCommandsInner(commands);
        if (size == sizeof(nint) && !type.IsValueType) {
            fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref oldValue)) {
                foreach (ChangedObserverContext context in newChunk.GetChangedObservers(type))
                    context.Invoker.Invoke(context.Observer, entity, inner, entityPointer, newChunk.Offsets, ref *vp);
            }
            return;
        }

        fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref oldValue)) {
            byte* ptr = (byte*)(Unsafe.Read<IntPtr>(vp) + sizeof(nint));
            foreach (ChangedObserverContext context in newChunk.GetChangedObservers(type))
                context.Invoker.Invoke(context.Observer, entity, inner, entityPointer, newChunk.Offsets, ref *ptr);
        }
    }

}
