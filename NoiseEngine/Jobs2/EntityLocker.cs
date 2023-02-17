using NoiseEngine.Collections;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Jobs2;

internal class EntityLocker {

    private readonly ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
    private readonly ManualResetEvent resetEvent = new ManualResetEvent(true);

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

            if (writeAccess)
                resetEvents[i] = chunk.GetLocker(entity.index).resetEvent;
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
                            l.ExitWriteLock();
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
                    resetEvents[i] = locker.resetEvent;

                    if (locker.locker.TryEnterWriteLock(1)) {
                        locker.resetEvent.Reset();

                        if (chunk != entity.chunk || index != entity.index) {
                            locker.locker.ExitWriteLock();
                        } else {
                            acquired.Add((locker, true));
                            continue;
                        }
                    }
                } else {
                    resetEvents[i] = null;
                    if (locker.locker.IsReadLockHeld) {
                        continue;
                    } else if (locker.locker.TryEnterReadLock(1)) {
                        locker.resetEvent.Reset();

                        if (chunk != entity.chunk || index != entity.index) {
                            locker.locker.ExitReadLock();
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
                        l.ExitWriteLock();
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
        resetEvent.Dispose();
    }

    public void EnterWriteLock() {
        resetEvent.Reset();
        locker.EnterWriteLock();
        resetEvent.Reset();
    }

    public void ExitWriteLock() {
        locker.ExitWriteLock();
        resetEvent.Set();
    }

    public void EnterReadLock() {
        resetEvent.Reset();
        locker.EnterReadLock();
        resetEvent.Reset();
    }

    public void ExitReadLock() {
        locker.ExitReadLock();
        if (locker.CurrentReadCount == 0)
            resetEvent.Set();
    }

}
