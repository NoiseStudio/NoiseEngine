using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly struct ColliderComponent : IComponent {

    private readonly PhysicsMaterial? material;
    private readonly ColliderComponentInner inner;

    public bool IsTrigger { get; init; }
    public ColliderType Type { get; }

    public PhysicsMaterial? Material {
        get => material;
        init {
            material = value;
            RestitutionPlusOneNegative = value?.Restitution ?? -1.1f;
        }
    }

    internal float RestitutionPlusOneNegative { get; private init; }

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
        Material = sphereCollider.Material;
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
        return new SphereCollider(IsTrigger, Material, inner.SphereRadius);
    }

    internal readonly Matrix3x3<float> ComputeComInertiaTensorMatrix(float mass) {
        return Type switch {
            ColliderType.Sphere => SphereCollider.ComputeComInertiaTensorMatrix(mass, inner.SphereRadius),
            _ => throw new NotImplementedException()
        };
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
