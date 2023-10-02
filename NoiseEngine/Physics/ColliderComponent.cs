using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Physics;

public readonly struct ColliderComponent : IComponent {

    private readonly PhysicsMaterial? material;
    private readonly object? referenceData;
    private readonly ColliderComponentInner inner;

    public bool IsTrigger { get; init; }
    public ColliderType Type { get; }

    public PhysicsMaterial? Material {
        get => material;
        init {
            material = value;
            RestitutionPlusOneNegative = value?.Restitution ?? -1.0f;
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

    public ColliderComponent(MeshCollider meshCollider) {
        IsTrigger = meshCollider.IsTrigger;
        Material = meshCollider.Material;
        Type = ColliderType.Mesh;
        referenceData = meshCollider.Data;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal readonly MeshCollider UnsafeCastToMeshCollider() {
        Debug.Assert(Type == ColliderType.Mesh);
        return new MeshCollider(IsTrigger, Material, Unsafe.As<MeshColliderData>(referenceData)!);
    }

    internal readonly Matrix3x3<float> ComputeComInertiaTensorMatrix(float mass) {
        return Type switch {
            ColliderType.Sphere => SphereCollider.ComputeComInertiaTensorMatrix(mass, inner.SphereRadius),
            ColliderType.Mesh => new Matrix3x3<float>(
                new float3(1 / 6f, 0, 0),
                new float3(0, 1 / 6f, 0),
                new float3(0, 0, 1 / 6f)
            ),
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

    /// <summary>
    /// Casts <paramref name="collider"/> to <see cref="MeshCollider"/>.
    /// </summary>
    /// <param name="collider"><see cref="ColliderComponent"/> to cast.</param>
    /// <exception cref="InvalidCastException">
    /// Thrown when <paramref name="collider"/>.<see cref="Type"/> is not a <see cref="ColliderType.Mesh"/>.
    /// </exception>
    public static explicit operator MeshCollider(ColliderComponent collider) {
        AssertType(collider, ColliderType.Mesh);
        return collider.UnsafeCastToMeshCollider();
    }

}
