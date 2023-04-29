﻿using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2.Affective;

internal partial class MockAffectiveSystemChildB : EntitySystem {

    public int Value { get; set; }

    private void OnUpdateEntity(ref MockAffectiveComponentA a, ref MockComponentD d) {
        a = a with {
            Precision = (MockAffectivePrecision)(((int)a.Precision + 1) % ((int)MockAffectivePrecision.High + 1))
        };
        d.Value = Value;
    }

}