using NoiseEngine.Jobs;
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
