using NoiseEngine.Mathematics;

namespace NoiseEngine.Rendering;

public interface ICameraRenderTarget {

    internal TextureUsage Usage { get; }
    internal TextureFormat Format { get; }
    internal Vector3<uint> Extent { get; }
    internal uint SampleCount { get; }

}
