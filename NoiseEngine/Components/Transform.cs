using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Components {
    public record struct Transform(Float3 Position, Quaternion Rotation, Float3 Scale) : IEntityComponent;
}
