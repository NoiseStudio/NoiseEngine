using System;

namespace NoiseEngine.Physics;

public readonly record struct MeshCollider(bool IsTrigger, PhysicsMaterial? Material, MeshColliderData Data) {

    /// <summary>
    /// Do not use default constructor for this type, always throws <see cref="InvalidOperationException"/>.
    /// Use another constructor instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Always throws <see cref="InvalidOperationException"/>.
    /// </exception>
    [Obsolete($"Do not use default constructor for this type. Use another constructor instead.", true)]
    public MeshCollider() : this(false, null, null!) {
        throw new InvalidOperationException(
            "Do not use default constructor for this type. Use another constructor instead."
        );
    }

    public MeshCollider(MeshColliderData data) : this(false, null, data) {
    }

    /// <summary>
    /// Casts <paramref name="component"/> to <see cref="ColliderComponent"/>.
    /// </summary>
    /// <param name="component"><see cref="MeshCollider"/> to cast.</param>
    public static implicit operator ColliderComponent(MeshCollider component) {
        return new ColliderComponent(component);
    }

}
