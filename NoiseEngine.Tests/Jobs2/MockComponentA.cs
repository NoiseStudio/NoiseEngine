using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2;

internal readonly record struct MockComponentA(string Text) : IComponent {

    public static MockComponentA TestValueA => new MockComponentA("The quick brown fox jumps over the lazy dog!");

}
