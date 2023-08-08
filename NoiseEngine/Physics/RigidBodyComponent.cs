using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

[AppendComponentDefault(typeof(RigidBodyFinalDataComponent))]
public record struct RigidBodyComponent : IComponent {

    private float mass = 1f;

    public Vector3<float> CenterOfMass { get; set; }
    public Vector3<float> LinearVelocity { get; set; }
    public Vector3<float> AngularVelocity { get; set; }

    public float Mass {
        readonly get => mass;
        set {
            mass = value;
            InverseMass = 1f / value;
        }
    }

    internal float InverseMass { get; private set; } = 1f;
    internal byte Sleeped { get; set; }

    public RigidBodyComponent() {
    }

}

