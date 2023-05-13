using NoiseEngine.Jobs2;
using System;

namespace NoiseEngine.Tests.Jobs2;

internal record MockComponentD : IComponent {

    public int Value { get; set; }

    public MockComponentD() { }

    public MockComponentD(int value) {
        Value = value;
    }

}
