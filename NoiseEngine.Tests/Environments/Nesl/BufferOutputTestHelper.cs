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

        neslType = NeslTestEnvironment.GetNeslTypeFromT<T>();
        shaderType = TestEmitHelper.NewType();

        shaderOutput = shaderType.DefineField("Output", singleOutput ? neslType : Buffers.GetReadWriteBuffer(neslType));
        shaderOutput.AddAttribute(UniformAttribute.Create());
    }

    public NeslFieldBuilder DefineField(NeslType type) {
        return shaderType.DefineField(Guid.NewGuid().ToString().Replace("-", ""), type);
    }

    public NeslMethodBuilder DefineMethod(NeslType? returnType = null, params NeslType[] parameterTypes) {
        return shaderType.DefineMethod(Guid.NewGuid().ToString(), returnType, parameterTypes);
    }

    public void ExecuteAndAssert(T[]? initialValues, params T[] expectedValues) {
        NeslMethod shaderMethod = shaderType.Methods.Single(x => x.Attributes.HasAnyAttribute(nameof(KernelAttribute)));

        T[] read = new T[expectedValues.Length];

        foreach (GraphicsDevice device in fixture.GraphicsDevices) {
            GraphicsHostBuffer<T> buffer = new GraphicsHostBuffer<T>(
                device, GraphicsBufferUsage.TransferAll | GraphicsBufferUsage.Storage,
                (ulong)Math.Max(initialValues?.Length ?? 0, expectedValues.Length)
            );

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
