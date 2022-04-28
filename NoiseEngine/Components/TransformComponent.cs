using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Components {
    public record struct TransformComponent(Float3 Position, Quaternion Rotation, Float3 Scale) : IEntityComponent {

        public TransformComponent(Float3 position, Quaternion rotation) : this(position, rotation, Float3.One) {
        }

        public TransformComponent(Float3 position) : this(position, Quaternion.Identity) {
        }

        public Matrix4x4 Matrix => Matrix4x4.Translate(Position) * Matrix4x4.Rotate(Rotation) * Matrix4x4.Scale(Scale);

    }
}
