using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Environments;

public abstract class NeslTestEnvironment : GraphicsTestEnvironment {

    protected NeslTestEnvironment(ApplicationFixture fixture) : base(fixture) {
    }

    private protected BufferOutputTestHelper<T> CreateBufferOutputTestHelper<T>(
        bool singleOutput = false
    ) where T : unmanaged {
        return new BufferOutputTestHelper<T>(Fixture, singleOutput);
    }

}
