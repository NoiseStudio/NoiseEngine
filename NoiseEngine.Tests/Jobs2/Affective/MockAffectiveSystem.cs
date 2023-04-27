using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2.Affective;

internal class MockAffectiveSystem : AffectiveSystem<MockAffectiveComponentA> {

    protected override EntitySystem Create(MockAffectiveComponentA componentA) {
        return new MockAffectiveSystemChild();
    }

}
