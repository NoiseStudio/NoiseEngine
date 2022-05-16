﻿using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System.Text;

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

            Matrix = default;
            CalculateMatrix();
        }

        public TransformComponent(Float3 position, Quaternion rotation) : this(position, rotation, Float3.One) {
        }

        public TransformComponent(Float3 position) : this(position, Quaternion.Identity) {
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true"/> if the current object is equal to the other parameter;
        /// otherwise, <see langword="false"/>.</returns>
        public bool Equals(TransformComponent other) {
            return Matrix.Equals(other.Matrix);
        }

        /// <summary>
        /// Returns hash code of this object.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() {
            return Matrix.GetHashCode();
        }

        private void CalculateMatrix() {
            Matrix = Matrix4x4.Translate(Position) * Matrix4x4.Rotate(Rotation) * Matrix4x4.Scale(Scale);
        }

        private bool PrintMembers(StringBuilder builder) {
            builder.Append($"{nameof(Position)} = {Position}, ");
            builder.Append($"{nameof(Rotation)} = {Rotation}, ");
            builder.Append($"{nameof(Scale)} = {Scale}");

            return true;
        }

    }
}