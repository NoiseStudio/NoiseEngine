namespace NoiseEngine.Physics;

public class PhysicsSettings {

    private const double DefaultGravity = -9.80665;

    private double gravity = DefaultGravity;

    public double Gravity {
        get => gravity;
        set {
            gravity = value;
            GravityF = (float)value;
        }
    }

    internal float GravityF { get; private set; } = (float)DefaultGravity;

}
