using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System.Text;

namespace NoiseEngine.Components;

public record struct TransformComponent : IEntityComponent {

    private readonly Float3 position;
    private readonly Quaternion rotation;
    private readonly Float3 scale;

    public static TransformComponent Default => new TransformComponent();

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

    public Float3 Left => Rotation * Float3.Left;
    public Float3 Right => Rotation * Float3.Right;
    public Float3 Up => Rotation * Float3.Up;
    public Float3 Down => Rotation * Float3.Down;
    public Float3 Front => Rotation * Float3.Front;
    public Float3 Back => Rotation * Float3.Back;

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

    public TransformComponent() : this(Float3.Zero) {
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
