using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System.Text;

namespace NoiseEngine.Components;

public record struct TransformComponent : IEntityComponent {

    private readonly Vector3<float> position;
    private readonly Quaternion<float> rotation;
    private readonly Vector3<float> scale;

    public static TransformComponent Default => new TransformComponent();

    public Vector3<float> Position {
        get => position;
        init {
            position = value;
            Matrix = CalculateMatrix();
        }
    }

    public Quaternion<float> Rotation {
        get => rotation;
        init {
            rotation = value;
            Matrix = CalculateMatrix();
        }
    }

    public Vector3<float> Scale {
        get => scale;
        init {
            scale = value;
            Matrix = CalculateMatrix();
        }
    }

    public Matrix4x4<float> Matrix { get; private init; }

    public Vector3<float> Left => Rotation * Vector3<float>.Left;
    public Vector3<float> Right => Rotation * Vector3<float>.Right;
    public Vector3<float> Up => Rotation * Vector3<float>.Up;
    public Vector3<float> Down => Rotation * Vector3<float>.Down;
    public Vector3<float> Front => Rotation * Vector3<float>.Front;
    public Vector3<float> Back => Rotation * Vector3<float>.Back;

    public TransformComponent(Vector3<float> position, Quaternion<float> rotation, Vector3<float> scale) {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;

        Matrix = CalculateMatrix();
    }

    public TransformComponent(Vector3<float> position, Quaternion<float> rotation)
        : this(position, rotation, Vector3<float>.One) {
    }

    public TransformComponent(Vector3<float> position) : this(position, Quaternion<float>.Identity) {
    }

    public TransformComponent() : this(Vector3<float>.Zero) {
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

    private Matrix4x4<float> CalculateMatrix() {
        return Matrix4x4<float>.Translate(Position) * Matrix4x4<float>.Rotate(Rotation) * Matrix4x4<float>.Scale(Scale);
    }

    private bool PrintMembers(StringBuilder builder) {
        builder.Append($"{nameof(Position)} = {Position}, ");
        builder.Append($"{nameof(Rotation)} = {Rotation}, ");
        builder.Append($"{nameof(Scale)} = {Scale}");

        return true;
    }

}
