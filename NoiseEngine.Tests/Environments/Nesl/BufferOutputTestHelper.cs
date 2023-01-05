using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Nesl;
using System;
using System.Linq;

namespace NoiseEngine.Tests.Environments.Nesl;

internal class BufferOutputTestHelper<T> where T : unmanaged {

    private readonly ApplicationFixture fixture;
    private readonly NeslType neslType;
    private readonly NeslTypeBuilder shaderType;
    private readonly NeslFieldBuilder shaderOutput;

    internal BufferOutputTestHelper(ApplicationFixture fixture, bool singleOutput) {
        this.fixture = fixture;

        if (typeof(T) == typeof(float))
            neslType = BuiltInTypes.Float32;
        else if (typeof(T) == typeof(double))
            neslType = BuiltInTypes.Float64;
        else if (typeof(T) == typeof(Vector4<float>))
            neslType = Vectors.GetVector4(BuiltInTypes.Float32);
        else
            throw new InvalidOperationException("Given generic T type is not supported.");

        shaderType = TestEmitHelper.NewType();

        shaderOutput = shaderType.DefineField("Output", singleOutput ? neslType : Buffers.GetReadWriteBuffer(neslType));
        shaderOutput.AddAttribute(UniformAttribute.Create());
    }

    public NeslMethodBuilder DefineMethod(NeslType? returnType = null, params NeslType[] parameterTypes) {
        return shaderType.DefineMethod(Guid.NewGuid().ToString(), returnType, parameterTypes);
    }

    public void ExecuteAndAssert(T[]? initialValues, params T[] expectedValues) {
        NeslMethod shaderMethod = shaderType.Methods.Single(x => x.Attributes.HasAnyAttribute(nameof(KernelAttribute)));

        T[] read = new T[expectedValues.Length];

        foreach (GraphicsDevice device in fixture.GraphicsDevices) {
            GraphicsHostBuffer<T> buffer =
               new GraphicsHostBuffer<T>(device, GraphicsBufferUsage.Storage, (ulong)expectedValues.Length);

            if (initialValues is not null)
                buffer.SetData(initialValues);

            ComputeShader shader = new ComputeShader(device, shaderType);
            shader.GetProperty(shaderOutput)!.SetBuffer(buffer);

            GraphicsCommandBuffer commandBuffer = fixture.GetCommandBuffer(device);
            commandBuffer.Dispatch(shader.GetKernel(shaderMethod)!, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            buffer.GetData(read);
            Assert.Equal(expectedValues, read);
        }
    }

}
