using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Nesl;
using System;

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
