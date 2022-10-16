using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Threading;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Rendering;

public abstract class GraphicsInstance : IDisposable {

    private AtomicBool isDisposed;

    public bool IsDisposed => isDisposed;
    public IReadOnlyList<GraphicsDevice> Devices => ProtectedDevices;

    protected abstract IReadOnlyList<GraphicsDevice> ProtectedDevices { get; set; }

    /// <summary>
    /// Creates new <see cref="GraphicsInstance"/>.
    /// </summary>
    /// <returns>New <see cref="GraphicsInstance"/>.</returns>
    public static GraphicsInstance Create() {
        return new VulkanInstance(new VulkanLibrary(), VulkanLogSeverity.All, VulkanLogType.All);
    }

    /// <summary>
    /// Disposes this <see cref="GraphicsInstance"/>.
    /// </summary>
    public void Dispose() {
        if (isDisposed.Exchange(true))
            return;

        ReleaseResources();

        ProtectedDevices = Array.Empty<GraphicsDevice>();
    }

    protected abstract void ReleaseResources();

}
