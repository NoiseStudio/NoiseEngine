using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

internal record MockComponentA(string Text, object? B) : IComponent {

    public static MockComponentA TestValueA => new MockComponentA("The quick brown fox jumps over the lazy dog!", null);
    public static MockComponentA TestValueB => new MockComponentA("kkard2 is too lazy.", null);

}
