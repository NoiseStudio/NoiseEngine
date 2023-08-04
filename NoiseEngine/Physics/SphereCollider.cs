using NoiseEngine.Mathematics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly record struct SphereCollider(bool IsTrigger = false, float Radius = 0.5f) {

    public SphereCollider() : this(false) {
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
