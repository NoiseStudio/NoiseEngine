using NoiseEngine.Interop;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering;

// TODO: Add property of driver version which returns Version. Different vendors have different implementations.
// https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPhysicalDeviceProperties.html
public abstract class GraphicsDevice {

    private readonly object initializeLocker = new object();
    private bool isInitialized;

    public GraphicsInstance Instance { get; }
    public string Name { get; }
    public GraphicsDeviceVendor Vendor { get; }
    public GraphicsDeviceType Type { get; }
    public Version ApiVersion { get; }
    public Guid Guid { get; }
    public bool IsSupportsGraphics { get; }
    public bool IsSupportsComputing { get; }

    internal InteropHandle<GraphicsDevice> Handle { get; }

    private protected GraphicsDevice(GraphicsInstance instance, GraphicsDeviceValue value) {
        Instance = instance;
        Name = value.Name;
        Vendor = value.Vendor;
        Type = value.Type;
        ApiVersion = value.ApiVersion;
        Guid = value.Guid;
        IsSupportsGraphics = value.IsSupportGraphics;
        IsSupportsComputing = value.IsSupportComputing;
        Handle = value.Handle;
    }

    /// <summary>
    /// Returns a string that represents the current <see cref="GraphicsDevice"/>.
    /// </summary>
    /// <returns>A string that represents the current <see cref="GraphicsDevice"/>.</returns>
    public override string ToString() {
        return $"{GetType().Name} {{ " +
            $"{nameof(Instance)} = {Instance}, " +
            $"{nameof(Name)} = \"{Name}\", " +
            $"{nameof(Vendor)} = {Vendor}, " +
            $"{nameof(Type)} = {Type}, " +
            $"{nameof(Guid)} = {Guid}, " +
            $"{nameof(Handle)} = {Handle} }}";
    }

    internal abstract void InternalDispose();

    internal void Initialize() {
        if (isInitialized)
            return;

        lock (initializeLocker) {
            if (isInitialized)
                return;

            InitializeWorker();
            isInitialized = true;
        }
    }

    internal VulkanDevice AsVulkan() {
        return (VulkanDevice)this;
    }

    protected abstract void InitializeWorker();

}
