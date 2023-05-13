using NoiseEngine.Jobs;

namespace NoiseEngine.Tests.Jobs;

[AppendComponentDefault(typeof(MockComponentB))]
internal record struct MockComponentF(decimal Value) : IComponent;
