using System.Text;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Components {
    public readonly record struct TransformComponent : IEntityComponent {

        private readonly Float3 position;
        private readonly Quaternion rotation;
        private readonly Float3 scale;

        public Float3 Position {
            get => position;
            init {
                position = value;
                Matrix = CalculateMatrix();
            }
        }

        public Quaternion Rotation {
            get => rotation;
            init {
                rotation = value;
                Matrix = CalculateMatrix();
            }
        }

        public Float3 Scale {
            get => scale;
            init {
                scale = value;
                Matrix = CalculateMatrix();
            }
        }

        public Matrix4x4 Matrix { get; private init; }

        public TransformComponent(Float3 position, Quaternion rotation, Float3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            Matrix = Matrix4x4.Translate(position) * Matrix4x4.Rotate(rotation) * Matrix4x4.Scale(scale);
        }

        public TransformComponent(Float3 position, Quaternion rotation) : this(position, rotation, Float3.One) {
        }

        public TransformComponent(Float3 position) : this(position, Quaternion.Identity) {
        }

        public bool Equals(TransformComponent other) {
            return Matrix.Equals(other.Matrix);
        }

        public override int GetHashCode() {
            return Matrix.GetHashCode();
        }

        private Matrix4x4 CalculateMatrix() {
            return Matrix4x4.Translate(Position) * Matrix4x4.Rotate(Rotation) * Matrix4x4.Scale(Scale);
        }

        private bool PrintMembers(StringBuilder builder) {
            builder.Append($"{nameof(Position)} = {Position}, ");
            builder.Append($"{nameof(Rotation)} = {Rotation}, ");
            builder.Append($"{nameof(Scale)} = {Scale}");
            return true;
        }

    }
}
