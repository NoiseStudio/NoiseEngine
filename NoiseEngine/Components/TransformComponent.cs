using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Components {
    public record struct TransformComponent : IEntityComponent {

        private Float3 position;
        private Quaternion rotation;
        private Float3 scale;

        public Float3 Position {
            get => position;
            set {
                position = value;
                CalculateMatrix();
            }
        }

        public Quaternion Rotation {
            get => rotation;
            set {
                rotation = value;
                CalculateMatrix();
            }
        }

        public Float3 Scale {
            get => scale;
            set {
                scale = value;
                CalculateMatrix();
            }
        }

        public Matrix4x4 Matrix { get; private set; }

        public TransformComponent(Float3 position, Quaternion rotation, Float3 scale) {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
            CalculateMatrix();
        }

        public TransformComponent(Float3 position, Quaternion rotation) : this(position, rotation, Float3.One) {
        }

        public TransformComponent(Float3 position) : this(position, Quaternion.Identity) {
        }

        private void CalculateMatrix() {
            Matrix = Matrix4x4.Translate(Position) * Matrix4x4.Rotate(Rotation) * Matrix4x4.Scale(Scale);
        }

    }
}
