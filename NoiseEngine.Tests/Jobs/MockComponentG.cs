using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

internal record struct MockComponentG(float ValueA, bool ValueB, int ValueC) : IComponent {

    public static MockComponentG TestValueA => new MockComponentG(114.114f, true, 456373525);

}
