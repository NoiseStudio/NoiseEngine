using NoiseEngine.Rendering.Vulkan;
using System.Collections.Generic;

namespace NoiseEngine.Rendering;

public abstract class GraphicsInstance {

    public abstract GraphicsApi Api { get; }
    public abstract bool SupportsPresentation { get; }

    public bool PresentationEnabled { get; }

    public IReadOnlyList<GraphicsDevice> Devices => ProtectedDevices;

    protected abstract IReadOnlyList<GraphicsDevice> ProtectedDevices { get; set; }

    private protected GraphicsInstance(bool presentationEnabled) {
        PresentationEnabled = presentationEnabled;
    }

    /// <summary>
    /// Creates new <see cref="GraphicsInstance"/>.
    /// </summary>
    /// <param name="disablePresentation">
    /// Specifies whether the instance should disable presentation (e.g. displaying a view on a window).
    /// </param>
    /// <param name="isDebug">Specifies whether the instance is to be used in debug mode.</param>
    /// <param name="enableValidationLayers">Specifies whether to use instance validation layers.</param>
    /// <returns>New <see cref="GraphicsInstance"/>.</returns>
    public static GraphicsInstance Create(bool disablePresentation, bool isDebug, bool enableValidationLayers) {
        VulkanLibrary library = new VulkanLibrary();

        return new VulkanInstance(
            library, isDebug ? VulkanLogSeverity.All : (VulkanLogSeverity.Warning | VulkanLogSeverity.Error),
            isDebug ? VulkanLogType.All : (
                enableValidationLayers ? (VulkanLogType.General | VulkanLogType.Validation
            ) : VulkanLogType.None), enableValidationLayers && library.SupportsValidationLayers,
            !disablePresentation && library.SupportsPresentation
        );
    }

}
