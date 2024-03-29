﻿using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs.Affective;

internal class MockAffectiveSystem<T> : AffectiveSystem<MockAffectiveComponentA> where T : EntitySystem, new() {

    protected override EntitySystem Create(MockAffectiveComponentA componentA) {
        return new T();
    }

}
