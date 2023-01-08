using NoiseEngine.Mathematics;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine;

public class Camera {

    private readonly CameraDelegation delegation;

    private ICameraRenderTarget? renderTarget;

    public ApplicationScene Scene { get; }
    public GraphicsDevice GraphicsDevice => Scene.GraphicsDevice;

    public CameraClearFlags ClearFlags { get; } = CameraClearFlags.SolidColor;
    public Vector3<float> BackgroundColor { get; } = new Vector3<float>(0, 1, 0);

    internal bool IsDirty { get; set; } = true;

    public ICameraRenderTarget? RenderTarget {
        get => renderTarget;
        set {
            renderTarget = value;
            IsDirty = true;

            if (value is null)
                delegation.ClearRenderTarget();
        }
    }

    public Camera(ApplicationScene scene) {
        Scene = scene;

        delegation = GraphicsDevice.Instance.Api switch {
            GraphicsApi.Vulkan => new VulkanCameraDelegation(this),
            _ => throw new GraphicsApiNotSupportedException(GraphicsDevice.Instance.Api),
        };
    }

}
