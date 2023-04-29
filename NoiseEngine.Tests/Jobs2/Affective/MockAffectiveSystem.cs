using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2.Affective;

internal class MockAffectiveSystem<T> : AffectiveSystem<MockAffectiveComponentA> where T : EntitySystem, new() {

    protected override EntitySystem Create(MockAffectiveComponentA componentA) {
        return new T();
    }

}
