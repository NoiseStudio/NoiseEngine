using NoiseEngine.Mathematics;

namespace NoiseEngine.Rendering;

public interface ICameraRenderTarget {

    internal TextureFormat Format { get; }
    internal Vector3<uint> Extent { get; }
    internal uint SampleCount { get; }

}
