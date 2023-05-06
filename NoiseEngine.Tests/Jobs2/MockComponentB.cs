using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2;

internal readonly record struct MockComponentB(float Value, string Text) : IComponent {

    public static MockComponentB TestValueA => new MockComponentB(01.23f, "Hello world!");
    public static MockComponentB TestValueB => new MockComponentB(04.11f, "Zero Two");

}
