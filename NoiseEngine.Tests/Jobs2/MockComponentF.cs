using NoiseEngine.Jobs2;

namespace NoiseEngine.Tests.Jobs2;

[AppendComponentDefault(typeof(MockComponentB))]
internal record struct MockComponentF(decimal Value) : IComponent;
