using NoiseEngine.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Jobs2.Commands;

internal class SystemCommandsExecutor {

    private readonly FastList<SystemCommand> commands;
    private readonly Dictionary<Type, (IComponent? value, int size, int affectiveHashCode)> components =
        new Dictionary<Type, (IComponent? value, int size, int affectiveHashCode)>();

    private int index;
    private EntityCommandsInner? entityCommands;
    private bool writeAccess;

    public SystemCommandsExecutor(FastList<SystemCommand> commands) {
        this.commands = commands;
    }

    public void Invoke() {
        while (index < commands.Count) {
            SystemCommand command = commands[index];
            switch (command.Type) {
                case SystemCommandType.GetEntity:
                    ProcessEntity();
                    components.Clear();
                    writeAccess = false;
                    entityCommands = (EntityCommandsInner)command.Value!;
                    break;
                case SystemCommandType.EntityDespawn:
                    DespawnEntity();
                    while (++index < commands.Count && commands[index].Type != SystemCommandType.GetEntity)
                        continue;
                    index--;
                    break;
                case SystemCommandType.EntityInsert:
                    writeAccess = true;
                    (IComponent component, int size, int affectiveHashCode) value =
                        ((IComponent, int, int))command.Value!;
                    Type type = value.component.GetType();
                    components[type] = value;
                    break;
                case SystemCommandType.EntityRemove:
                    Type typeB = (Type)command.Value!;
                    writeAccess |= entityCommands!.Entity.Contains(typeB);
                    components[typeB] = (null, 0, 0);
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
            foreach ((Type type, (IComponent? value, _, _)) in components) {
                if ((value is null && entityCommands.Entity.Contains(type)) || !entityCommands.Entity.Contains(type)) {
                    held.Dispose();
                    writeAccess = true;
                    ProcessEntity();
                    return;
                }
            }
        }

        if (entityCommands.ConditionalsCount == 0) {
            entity.TryGetWorld(out EntityWorld? world);
            Archetype newArchetype = world!.GetArchetype(
                components.Where(x => x.Value.value is not null).Select(x => (x.Key, x.Value.affectiveHashCode))
                .UnionBy(entity.chunk!.Archetype.ComponentTypes.Select(x => (x.type, x.affectiveHashCode)).Where(
                    x => !components.TryGetValue(x.type, out (IComponent? value, int size, int affectiveHashCode) o) ||
                        o.value is not null
                ), x => x.Item1),
                () => components.Where(x => x.Value.value is not null)
                .Select(x => (x.Key, x.Value.size, x.Value.affectiveHashCode))
                .UnionBy(entity.chunk!.Archetype.ComponentTypes.Where(
                    x => !components.TryGetValue(x.type, out (IComponent? value, int size, int affectiveHashCode) o) ||
                    o.value is not null
                ), x => x.Item1).ToArray()
            );

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
        (ArchetypeChunk newChunk, nint newIndex) = newArchetype.TakeRecord();

        entity.chunk = newChunk;
        nint oldIndex = entity.index;
        entity.index = newIndex;

        unsafe {
            fixed (byte* dp = newChunk.StorageData) {
                byte* di = dp + newIndex;
                fixed (byte* sp = oldChunk.StorageData) {
                    byte* si = sp + oldIndex;

                    // Copy old components.
                    foreach ((Type type, int size, _) in newChunk.Archetype.ComponentTypes) {
                        (IComponent? value, int size, int affectiveHashCode) component;
                        if (oldChunk.Offsets.TryGetValue(type, out nint oldOffset)) {
                            if (!components.TryGetValue(type, out component)) {
                                Buffer.MemoryCopy(si + oldOffset, di + newChunk.Offsets[type], size, size);
                                continue;
                            }
                        } else {
                            component = components[type];
                        }

                        fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref component.value!)) {
                            Buffer.MemoryCopy(
                                (void*)(Unsafe.Read<IntPtr>(vp) + sizeof(nint)),
                                di + newChunk.Offsets[type], size, size
                            );
                        }
                    }

                    // Copy internal component.
                    int iSize = Unsafe.SizeOf<EntityInternalComponent>();
                    Buffer.MemoryCopy(si, di, iSize, iSize);

                    // Clear old data.
                    new Span<byte>(si, (int)oldChunk.Archetype.RecordSize).Clear();
                }
            }
        }

        held.Dispose();
        oldChunk.Archetype.ReleaseRecord(oldChunk, oldIndex);
    }

    private void UpdateRecord(EntityLockerHeld held) {
        Entity entity = entityCommands!.Entity;
        ArchetypeChunk chunk = entity.chunk!;

        unsafe {
            fixed (byte* dp = chunk.StorageData) {
                byte* di = dp + entity.index;

                // Update components.
                foreach ((Type type, (IComponent? value, int size, int affectiveHashCode)) in components) {
                    if (value is null)
                        continue;

                    IComponent component = value;
                    fixed (byte* vp = &Unsafe.As<IComponent, byte>(ref component)) {
                        Buffer.MemoryCopy(
                            (void*)(Unsafe.Read<IntPtr>(vp) + sizeof(nint)), di + chunk.Offsets[type], size, size
                        );
                    }
                }
            }
        }

        held.Dispose();
    }

}
