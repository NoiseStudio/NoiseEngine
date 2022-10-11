using NoiseEngine.Threading;
using System;

namespace NoiseEngine.Rendering;

internal abstract class GraphicsInstance : IDisposable {

    private readonly AtomicBool isDisposed;

    public bool IsDisposed => isDisposed;

    public void Dispose() {
        if (!isDisposed.Exchange(true))
            ReleaseResources();
    }

    protected abstract void ReleaseResources();

}
