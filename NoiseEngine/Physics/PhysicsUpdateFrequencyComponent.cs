using NoiseEngine.Jobs;

namespace NoiseEngine.Physics;

public readonly record struct PhysicsUpdateFrequencyComponent(
    PhysicsUpdateFrequency Frequency
) : IAffectiveComponent<PhysicsUpdateFrequencyComponent> {

    bool IAffectiveComponent<PhysicsUpdateFrequencyComponent>.AffectiveEquals(PhysicsUpdateFrequencyComponent other) {
        return Frequency == other.Frequency;
    }

    int IAffectiveComponent.GetAffectiveHashCode() {
        return (int)Frequency;
    }

}
