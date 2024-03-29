﻿using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

internal record MockComponentD : IComponent {

    public int Value { get; set; }

    public MockComponentD() { }

    public MockComponentD(int value) {
        Value = value;
    }

}
