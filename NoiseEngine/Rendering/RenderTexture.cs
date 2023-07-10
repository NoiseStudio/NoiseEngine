using NoiseEngine.Mathematics;
using System;
using System.Threading;

namespace NoiseEngine.Rendering;

public class RenderTexture : ICameraRenderTarget {

    private Texture? depthStencil;

    public GraphicsDevice Device => Color.Device;
    public Texture Color { get; }

    public Texture DepthStencil {
        get {
            if (depthStencil is null) {
                Interlocked.CompareExchange(ref depthStencil, new Texture2D(
                    Device, TextureUsage.DepthStencilAttachment, Color.Extent.X, Color.Extent.Y,
                    TextureFormat.D32_SFloat
                ), null);
            }
            return depthStencil;
        }
    }

    Vector3<uint> ICameraRenderTarget.Extent => Color.Extent;

    public RenderTexture(Texture color) {
        Color = color;

        if (!color.Usage.HasFlag(TextureUsage.ColorAttachment)) {
            throw new ArgumentException(
                $"{nameof(Color)} texture must have TextureUsage.ColorAttachment flag.", nameof(color)
            );
        }
        if (!color.Usage.HasFlag(TextureUsage.TransferDestination)) {
            throw new ArgumentException(
                $"{nameof(Color)} texture must have TextureUsage.TransferDestination flag.", nameof(color)
            );
        }
    }

    public RenderTexture(Texture color, Texture depthStencil) : this(color) {
        this.depthStencil = depthStencil;

        if (!depthStencil.Usage.HasFlag(TextureUsage.DepthStencilAttachment)) {
            throw new ArgumentException(
                $"{nameof(DepthStencil)} texture must have TextureUsage.DepthStencilAttachment flag.",
                nameof(depthStencil)
            );
        }
    }

}
