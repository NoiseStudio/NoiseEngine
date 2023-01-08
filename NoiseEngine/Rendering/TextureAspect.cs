namespace NoiseEngine.Rendering;

internal enum TextureAspect : uint {
    Color = 1 << 0,
    Depth = 1 << 1,
    Stencil = 1 << 2
}
