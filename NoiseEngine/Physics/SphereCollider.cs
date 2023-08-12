using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly record struct SphereCollider(bool IsTrigger = false, float Radius = 0.5f) {

    public SphereCollider() : this(false) {
    }

    internal static Matrix3x3<float> ComputeComInertiaTensorMatrix(float mass, float radius) {
        float a = radius * radius * mass * 0.4f;
        return new Matrix3x3<float>(
            new Vector3<float>(a, 0, 0),
            new Vector3<float>(0, a, 0),
            new Vector3<float>(0, 0, a)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float ScaledRadius(Vector3<float> scale) {
        return Radius * scale.MaxComponent();
    }

    /// <summary>
    /// Casts <paramref name="component"/> to <see cref="ColliderComponent"/>.
    /// </summary>
    /// <param name="component"><see cref="SphereCollider"/> to cast.</param>
    public static implicit operator ColliderComponent(SphereCollider component) {
        return new ColliderComponent(component);
    }

}
