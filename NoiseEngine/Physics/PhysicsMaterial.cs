using System;

namespace NoiseEngine.Physics;

public class PhysicsMaterial {

    private readonly float restitution = 0.1f;

    public float Restitution {
        get => restitution;
        init {
            restitution = Math.Clamp(value, 0f, 1f);
            RestitutionPlusOneNegative = -(1f + restitution);
        }
    }

    /// <summary>
    /// -(1f + <see cref="Restitution"/>)
    /// </summary>
    internal float RestitutionPlusOneNegative { get; private set; }

}
