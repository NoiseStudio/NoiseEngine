using System;

namespace NoiseEngine.Common;

internal interface IReferenceCoutable {

    public bool TryRcRetain();
    public void RcRelease();

    public void RcRetain() {
        if (!TryRcRetain())
            throw new ObjectDisposedException(GetType().FullName, "Unable to retain reference counter.");
    }

}
