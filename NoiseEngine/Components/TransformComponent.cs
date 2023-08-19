using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System.Text;

namespace NoiseEngine.Components;

public record struct TransformComponent : IComponent {

    private readonly pos3 position;
    private readonly Quaternion<float> rotation;
    private readonly float3 scale;

    public static TransformComponent Default => new TransformComponent();

    public pos3 Position {
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

    public float3 Scale {
        get => scale;
        init {
            scale = value;
            Matrix = CalculateMatrix();
        }
    }

    public Matrix4x4<pos> Matrix { get; private init; }

    public Vector3<float> Left => Rotation * Vector3<float>.Left;
    public Vector3<float> Right => Rotation * Vector3<float>.Right;
    public Vector3<float> Up => Rotation * Vector3<float>.Up;
    public Vector3<float> Down => Rotation * Vector3<float>.Down;
    public Vector3<float> Front => Rotation * Vector3<float>.Front;
    public Vector3<float> Back => Rotation * Vector3<float>.Back;

    public TransformComponent(pos3 position, Quaternion<float> rotation, float3 scale) {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;

        Matrix = CalculateMatrix();
    }

    public TransformComponent(pos3 position, Quaternion<float> rotation)
        : this(position, rotation, float3.One) {
    }

    public TransformComponent(pos3 position) : this(position, Quaternion<float>.Identity) {
    }

    public TransformComponent() : this(pos3.Zero) {
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

    private Matrix4x4<pos> CalculateMatrix() {
        return Matrix4x4<pos>.Translate(Position) * Matrix4x4<pos>.Rotate(Rotation.ToPos()) *
            Matrix4x4<pos>.Scale(Scale.ToPos());
    }

    private bool PrintMembers(StringBuilder builder) {
        builder.Append($"{nameof(Position)} = {Position}, ");
        builder.Append($"{nameof(Rotation)} = {Rotation}, ");
        builder.Append($"{nameof(Scale)} = {Scale}");

        return true;
    }

}
