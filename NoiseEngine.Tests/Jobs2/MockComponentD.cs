using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2;

internal readonly record struct MockComponentD(
    MockAffectivePrecision Precision
) : IAffectiveComponent<MockComponentD> {

    bool IAffectiveComponent<MockComponentD>.AffectiveEquals(MockComponentD other) {
        return other.Precision == Precision;
    }

    int IAffectiveComponent.GetAffectiveHashCode() {
        return Precision.GetHashCode();
    }

}
