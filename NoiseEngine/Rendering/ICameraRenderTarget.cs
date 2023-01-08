namespace NoiseEngine.Rendering;

public interface ICameraRenderTarget {

    internal TextureFormat Format { get; }
    internal uint SampleCount { get; }

}
