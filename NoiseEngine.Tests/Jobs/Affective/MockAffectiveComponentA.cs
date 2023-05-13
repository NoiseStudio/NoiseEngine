using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs.Affective;

internal readonly record struct MockAffectiveComponentA(
    MockAffectivePrecision Precision
) : IAffectiveComponent<MockAffectiveComponentA> {

    public static MockAffectiveComponentA Low => new MockAffectiveComponentA(MockAffectivePrecision.Low);
    public static MockAffectiveComponentA Medium => new MockAffectiveComponentA(MockAffectivePrecision.Medium);
    public static MockAffectiveComponentA High => new MockAffectiveComponentA(MockAffectivePrecision.High);

    bool IAffectiveComponent<MockAffectiveComponentA>.AffectiveEquals(MockAffectiveComponentA other) {
        return other.Precision == Precision;
    }

    int IAffectiveComponent.GetAffectiveHashCode() {
        return Precision.GetHashCode();
    }

}
