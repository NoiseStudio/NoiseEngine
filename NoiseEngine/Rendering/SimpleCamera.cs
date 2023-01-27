using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;
using System;
using System.ComponentModel;

namespace NoiseEngine.Rendering;

public class SimpleCamera {

    private readonly object renderTargetLocker = new object();

    private CameraClearFlags clearFlags = CameraClearFlags.SolidColor;
    private Color clearColor = new Color(0.26666f, 0.45882f, 0.87058f);
    private ProjectionType projectionType = ProjectionType.Perspective;
    private Vector3<float> position = new Vector3<float>(0, 0, -2.5f);
    private Quaternion<float> rotation = Quaternion<float>.Identity;
    private float nearClipPlane = 0.1f;
    private float farClipPlane = 1000f;
    private float fieldOfView = FloatingPointIeee754Helper.ConvertDegreesToRadians(60f);
    private float orthographicSize = 10.0f;
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
            if (renderTarget == value)
                return;
            AssertRenderTarget(value);

            lock (renderTargetLocker) {
                if (value is Window window)
                    window.ChangeAssignedCamera(this);

                RaiseRenderTargetSet(value);

                if (value is null)
                    Delegation.RaiseRenderTargetSet(value);

                IsDirty = true;
                renderTarget = value;
            }
        }
    }

    public float AspectRatio {
        get {
            ICameraRenderTarget? renderTarget = RenderTarget;
            if (renderTarget is null)
                return float.NaN;

            Vector3<uint> extent = renderTarget.Extent;
            return extent.X / (float)extent.Y;
        }
    }

    public Matrix4x4<float> ViewMatrix => CalculateViewMatrix();
    public Matrix4x4<float> ProjectionMatrix => CalculateProjectionMatrix();

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

    internal void CompareExchangeRenderTarget(ICameraRenderTarget? value, ICameraRenderTarget? comparand) {
        lock (renderTargetLocker) {
            if (renderTarget != comparand)
                return;
            RenderTarget = value;
        }
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

    private Matrix4x4<float> CalculateViewMatrix() {
        // TODO: Implement inverse quaternion in NoiseEngine.Mathematics
        float ls =
            rotation.X * rotation.X + rotation.Y * rotation.Y + rotation.Z * rotation.Z + rotation.W * rotation.W;
        float inverseNormal = 1.0f / ls;
        Quaternion<float> inverseRotation = new Quaternion<float> {
            X = -rotation.X * inverseNormal,
            Y = -rotation.Y * inverseNormal,
            Z = -rotation.Z * inverseNormal,
            W = rotation.W * inverseNormal
        };

        return Matrix4x4<float>.Rotate(inverseRotation) * Matrix4x4<float>.Translate(Vector3<float>.Zero - position);
    }

    private Matrix4x4<float> CalculateProjectionMatrix() {
        float farMinusNear = farClipPlane - nearClipPlane;

        switch (projectionType) {
            case ProjectionType.Perspective:
                float tanHalfFieldOfView = MathF.Tan(fieldOfView * 0.5f);
                float zRange = nearClipPlane - farClipPlane;

                return new Matrix4x4<float>(
                    new Vector4<float>(1 / (AspectRatio * tanHalfFieldOfView), 0.0f, 0.0f, 0.0f),
                    new Vector4<float>(0.0f, -1 / tanHalfFieldOfView, 0.0f, 0.0f),
                    new Vector4<float>(0.0f, 0.0f, (-nearClipPlane - farClipPlane) / zRange, 1.0f),
                    new Vector4<float>(0.0f, 0.0f, 2.0f * farClipPlane * nearClipPlane / zRange, 0.0f));

            case ProjectionType.Orthographic:
                return new Matrix4x4<float>(
                    new Vector4<float>(1 / (orthographicSize * AspectRatio), 0, 0, 0),
                    new Vector4<float>(0, -1 / orthographicSize, 0, 0),
                    new Vector4<float>(0, 0, 1 / farMinusNear, 0),
                    new Vector4<float>(0, 0, 0.5f * (-(farClipPlane + nearClipPlane) / farMinusNear + 1), 1));

            default:
                throw new InvalidEnumArgumentException(
                    nameof(ProjectionType),
                    (int)projectionType,
                    typeof(ProjectionType));
        }
    }

}

