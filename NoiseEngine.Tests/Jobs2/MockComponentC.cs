using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2;

internal readonly record struct MockComponentC(byte A, MockComponentA Inner, byte B) : IComponent {

    public static MockComponentC TestValueA => new MockComponentC(75, MockComponentA.TestValueA, 16);

}
