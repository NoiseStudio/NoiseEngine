namespace NoiseEngine.Physics;

public readonly record struct SphereCollider(bool IsTrigger = false, float Radius = 1f) {

    public SphereCollider() : this(false) {
    }

    /// <summary>
    /// Casts <paramref name="component"/> to <see cref="ColliderComponent"/>.
    /// </summary>
    /// <param name="component"><see cref="SphereCollider"/> to cast.</param>
    public static implicit operator ColliderComponent(SphereCollider component) {
        return new ColliderComponent(component);
    }

}
