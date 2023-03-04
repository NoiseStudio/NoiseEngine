using NoiseEngine.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal class EntityLocker {

    private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
    private readonly ManualResetEvent writeResetEvent = new ManualResetEvent(true);
    private readonly ManualResetEvent readResetEvent = new ManualResetEvent(true);

    public static bool TryLockEntity(Entity entity, bool writeAccess, out EntityLockerHeld held) {
        return TryLockEntities(new (Entity entity, bool writeAccess)[] { (entity, writeAccess) }, out held);
    }

    public static bool TryLockEntities((Entity entity, bool writeAccess)[] entities, out EntityLockerHeld held) {
        ManualResetEvent?[] resetEvents = new ManualResetEvent?[entities.Length];
        FastList<(EntityLocker, bool)> acquired = new FastList<(EntityLocker, bool)>(entities.Length);

        for (int i = 0; i < entities.Length; i++) {
            (Entity entity, bool writeAccess) = entities[i];
            ArchetypeChunk? chunk = entity.chunk;
            if (chunk is null) {
                held = default;
                return false;
            }

            EntityLocker locker = chunk.GetLocker(entity.index);
            resetEvents[i] = writeAccess ? locker.writeResetEvent : locker.readResetEvent;
        }

        do {
            WaitHandle.WaitAll(resetEvents.Where(x => x is not null).Distinct().ToArray()!);

            for (int i = 0; i < entities.Length; i++) {
                (Entity entity, bool writeAccess) = entities[i];
                ArchetypeChunk? chunk = entity.chunk;
                if (chunk is null) {
                    foreach ((EntityLocker l, bool wa) in acquired) {
                        if (wa)
                            l.ExitWriteLock();
                        else
                            l.ExitReadLock();
                    }
                    acquired.Clear();

                    held = default;
                    return false;
                }

                nint index = entity.index;
                EntityLocker locker = chunk.GetLocker(entity.index);

                if (writeAccess) {
                    if (locker.locker.IsWriteLockHeld)
                        continue;
                    resetEvents[i] = locker.writeResetEvent;

                    if (locker.TryEnterWriteLock(1)) {
                        if (chunk != entity.chunk || index != entity.index) {
                            locker.ExitWriteLock();
                        } else {
                            acquired.Add((locker, true));
                            continue;
                        }
                    }
                } else {
                    if (locker.locker.IsReadLockHeld)
                        continue;
                    resetEvents[i] = locker.readResetEvent;

                    if (locker.TryEnterReadLock(1)) {
                        if (chunk != entity.chunk || index != entity.index) {
                            locker.ExitReadLock();
                        } else {
                            acquired.Add((locker, false));
                            continue;
                        }
                    }
                }

                foreach ((EntityLocker l, bool wa) in acquired) {
                    if (wa)
                        l.ExitWriteLock();
                    else
                        l.ExitReadLock();
                }
                acquired.Clear();
                break;
            }
        } while (acquired.Count != entities.Length);

        held = new EntityLockerHeld(acquired);
        return true;
    }

    ~EntityLocker() {
        locker.Dispose();
        writeResetEvent.Dispose();
        readResetEvent.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEnterWriteLock(int millisecondsTimeout) {
        if (locker.TryEnterWriteLock(millisecondsTimeout)) {
            readResetEvent.Reset();
            writeResetEvent.Reset();
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExitWriteLock() {
        locker.ExitWriteLock();
        writeResetEvent.Set();
        readResetEvent.Set();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryEnterReadLock(int millisecondsTimeout) {
        if (locker.TryEnterReadLock(millisecondsTimeout)) {
            readResetEvent.Set();
            writeResetEvent.Reset();
            return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExitReadLock() {
        locker.ExitReadLock();
        if (locker.CurrentReadCount == 0)
            writeResetEvent.Set();
    }

}
