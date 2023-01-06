using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Environments;

public abstract class NeslTestEnvironment : GraphicsTestEnvironment {

    protected NeslTestEnvironment(ApplicationFixture fixture) : base(fixture) {
    }

    internal static NeslType GetNeslTypeFromT<T>() {
        if (typeof(T) == typeof(uint))
            return BuiltInTypes.UInt32;

        if (typeof(T) == typeof(int))
            return BuiltInTypes.Int32;

        if (typeof(T) == typeof(float))
            return BuiltInTypes.Float32;
        if (typeof(T) == typeof(double))
            return BuiltInTypes.Float64;

        if (typeof(T) == typeof(Vector4<uint>))
            return Vectors.GetVector4(BuiltInTypes.UInt32);

        if (typeof(T) == typeof(Vector4<int>))
            return Vectors.GetVector4(BuiltInTypes.Int32);

        if (typeof(T) == typeof(Vector4<float>))
            return Vectors.GetVector4(BuiltInTypes.Float32);

        throw new InvalidOperationException("Given generic T type is not supported.");
    }

    private protected BufferOutputTestHelper<T> CreateBufferOutputTestHelper<T>(
        bool singleOutput = false
    ) where T : unmanaged {
        return new BufferOutputTestHelper<T>(Fixture, singleOutput);
    }

}
