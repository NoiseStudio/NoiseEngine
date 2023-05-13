using NoiseEngine.Collections;
using System;

namespace NoiseEngine.Jobs2;

internal struct EntityLockerHeld : IDisposable {

    private FastList<(EntityLocker, bool)>? acquired;

    public EntityLockerHeld(FastList<(EntityLocker, bool)> acquired) {
        this.acquired = acquired;
    }

    public void Dispose() {
        foreach ((EntityLocker l, bool wa) in acquired!) {
            if (wa)
                l.ExitWriteLock();
            else
                l.ExitReadLock();
        }
        acquired = null;
    }

}
