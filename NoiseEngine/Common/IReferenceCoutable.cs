using System;

namespace NoiseEngine.Common;

internal interface IReferenceCoutable {

    public const long DisposeReferenceCount = long.MinValue / 2;

    public bool TryRcRetain();
    public void RcRelease();

    public void RcRetain() {
        if (!TryRcRetain())
            throw new ObjectDisposedException(GetType().FullName, "Unable to retain reference counter.");
    }

}
