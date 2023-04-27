using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2.Affective;

internal readonly record struct MockAffectiveComponentA(
    MockAffectivePrecision Precision
) : IAffectiveComponent<MockAffectiveComponentA> {

    bool IAffectiveComponent<MockAffectiveComponentA>.AffectiveEquals(MockAffectiveComponentA other) {
        return other.Precision == Precision;
    }

    int IAffectiveComponent.GetAffectiveHashCode() {
        return Precision.GetHashCode();
    }

}
