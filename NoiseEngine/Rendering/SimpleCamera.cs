using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering;

public class SimpleCamera {

    private CameraClearFlags clearFlags = CameraClearFlags.SolidColor;
    private Color clearColor = new Color(0.26666f, 0.45882f, 0.87058f);
    private ICameraRenderTarget? renderTarget;

    public GraphicsDevice GraphicsDevice { get; }

    public CameraClearFlags ClearFlags {
        get => clearFlags;
        set {
            clearFlags = value;
            IsDirty = true;
        }
    }

    public Color ClearColor {
        get => clearColor;
        set {
            clearColor = value;
            Delegation.UpdateClearColor();
        }
    }

    /// <summary>
    /// Sets camera render target. If setted render target is not null, texture usage of this render target must have
    /// TextureUsage.ColorAttachment flag. Also when render target is <see cref="Window"/>, <see cref="GraphicsDevice"/>
    /// must supports presentation.
    /// </summary>
    public ICameraRenderTarget? RenderTarget {
        get => renderTarget;
        set {
            AssertRenderTarget(value);

            renderTarget = value;
            IsDirty = true;

            if (value is null)
                Delegation.ClearRenderTarget();

            RaiseRenderTargetSet(value);
        }
    }

    internal SimpleCameraDelegation Delegation { get; }
    internal bool IsDirty { get; set; } = true;

    public SimpleCamera(GraphicsDevice graphicsDevice) {
        GraphicsDevice = graphicsDevice;

        Delegation = GraphicsDevice.Instance.Api switch {
            GraphicsApi.Vulkan => new VulkanSimpleCameraDelegation(this),
            _ => throw new GraphicsApiNotSupportedException(GraphicsDevice.Instance.Api),
        };

        Delegation.UpdateClearColor();
    }

    private protected virtual void RaiseRenderTargetSet(ICameraRenderTarget? newRenderTarget) {
    }

    private void AssertRenderTarget(ICameraRenderTarget? renderTarget) {
        if (renderTarget is null)
            return;

        if (!renderTarget.Usage.HasFlag(TextureUsage.ColorAttachment)) {
            throw new InvalidOperationException(
                $"{ToString()} render target must have TextureUsage.ColorAttachment flag."
            );
        }

        if (renderTarget is Window) {
            if (!GraphicsDevice.Instance.PresentationEnabled) {
                if (!GraphicsDevice.Instance.SupportsPresentation) {
                    throw new PresentationNotSupportedException(
                        $"{nameof(GraphicsInstance)} used by {ToString()} is not support presentation."
                    );
                } else {
                    throw new PresentationNotSupportedException(
                        $"{nameof(GraphicsInstance)} used by {ToString()} has disabled presentation."
                    );
                }
            }

            if (!GraphicsDevice.SupportsPresentation) {
                throw new PresentationNotSupportedException(
                    $"{nameof(GraphicsDevice)} used by {ToString()} is not support presentation."
                );
            }
        }
    }

}

