using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly struct ColliderComponent : IComponent {

    private readonly ColliderComponentInner inner;

    public bool IsTrigger { get; init; }
    public ColliderType Type { get; }

    /// <summary>
    /// Do not use default constructor for this type, always throws <see cref="InvalidOperationException"/>.
    /// Use another constructor instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Always throws <see cref="InvalidOperationException"/>.
    /// </exception>
    [Obsolete($"Do not use default constructor for this type. Use another constructor instead.", true)]
    public ColliderComponent() {
        throw new InvalidOperationException(
            "Do not use default constructor for this type. Use another constructor instead."
        );
    }

    public ColliderComponent(SphereCollider sphereCollider) {
        IsTrigger = sphereCollider.IsTrigger;
        Type = ColliderType.Sphere;
        inner = new ColliderComponentInner(sphereCollider);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AssertType(ColliderComponent collider, ColliderType type) {
        if (collider.Type != type)
            throw new InvalidCastException($"Cannot cast {collider.Type} to {type}.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly SphereCollider UnsafeCastToSphereCollider() {
        Debug.Assert(Type == ColliderType.Sphere);
        return new SphereCollider(IsTrigger, inner.SphereRadius);
    }

    internal readonly Vector3<float> ComputeInertiaTensor(Vector3<float> centerOfMass, float mass) {
        Vector3<float> comInertiaTensor = Type switch {
            ColliderType.Sphere => SphereCollider.ComputeComInertiaTensor(mass, inner.SphereRadius),
            _ => throw new NotImplementedException()
        };

        // Move inertia tensor by center of mass.
        // https://en.wikipedia.org/wiki/Parallel_axis_theorem#Tensor_generalization
        float r = centerOfMass.Dot(Vector3<float>.Zero);
        if (r == 0)
            return comInertiaTensor;
        r *= r;

        // Ignore multiplication by Kronecker delta, because the center of mass component is always equals to itself.
        // TODO: Check if Ri*Rj is always centerOfMass.X^2.
        return new Vector3<float>(
            comInertiaTensor.X + mass * (r - centerOfMass.X * centerOfMass.X),
            comInertiaTensor.Y + mass * (r - centerOfMass.Y * centerOfMass.Y),
            comInertiaTensor.Z + mass * (r - centerOfMass.Z * centerOfMass.Z)
        );
    }

    /// <summary>
    /// Casts <paramref name="collider"/> to <see cref="SphereCollider"/>.
    /// </summary>
    /// <param name="collider"><see cref="ColliderComponent"/> to cast.</param>
    /// <exception cref="InvalidCastException">
    /// Thrown when <paramref name="collider"/>.<see cref="Type"/> is not a <see cref="ColliderType.Sphere"/>.
    /// </exception>
    public static explicit operator SphereCollider(ColliderComponent collider) {
        AssertType(collider, ColliderType.Sphere);
        return collider.UnsafeCastToSphereCollider();
    }

}
