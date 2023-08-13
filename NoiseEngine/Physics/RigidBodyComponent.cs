using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Physics;

[AppendComponentDefault(typeof(RigidBodyFinalDataComponent), typeof(RigidBodySleepComponent))]
public record struct RigidBodyComponent : IComponent {

    internal const int SleepThreshold = 30;
    internal const int MaxSleepAccumulator = SleepThreshold + 2;

    private float mass = 1f;

    public bool UseGravity { get; set; } = true;
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

    public Matrix3x3<float> InertiaTensorMatrix {
        readonly get {
            if (InverseInertiaTensorMatrix.TryInvert(out Matrix3x3<float> result))
                return result;
            return InverseInertiaTensorMatrix;
        }
        set {
            if (value.TryInvert(out Matrix3x3<float> result))
                InverseInertiaTensorMatrix = result;
            else
                InverseInertiaTensorMatrix = value;
        }
    }

    internal Matrix3x3<float> InverseInertiaTensorMatrix { get; private set; }
    internal float InverseMass { get; private set; } = 1f;
    internal int SleepAccumulator { get; set; }

    internal bool IsSleeping => SleepAccumulator >= SleepThreshold;

    public RigidBodyComponent() {
    }

    internal void RecalculateProperties(Entity thisEntity) {
        if (!thisEntity.TryGet(out ColliderComponent collider))
            return;

        InertiaTensorMatrix = ComputeInertiaTensorMatrix(collider.ComputeComInertiaTensorMatrix(Mass));
    }

    private readonly Matrix3x3<float> ComputeInertiaTensorMatrix(Matrix3x3<float> comInertiaTensor) {
        // Move inertia tensor by center of mass.
        // https://en.wikipedia.org/wiki/Parallel_axis_theorem#Tensor_generalization
        float r = CenterOfMass.Dot(Vector3<float>.Zero);
        if (r == 0)
            return comInertiaTensor;
        r *= r;

        // Ignore multiplication by Kronecker delta, because the center of mass component is always equals to itself.
        // TODO: Check if Ri*Rj is always centerOfMass.X^2.
        return comInertiaTensor with {
            C1 = comInertiaTensor.C1 with {
                X = comInertiaTensor.M11 + mass * (r - CenterOfMass.X * CenterOfMass.X)
            },
            C2 = comInertiaTensor.C2 with {
                Y = comInertiaTensor.M22 + mass * (r - CenterOfMass.Y * CenterOfMass.Y),
            },
            C3 = comInertiaTensor.C3 with {
                Z = comInertiaTensor.M33 + mass * (r - CenterOfMass.Z * CenterOfMass.Z)
            }
        };
    }

}

