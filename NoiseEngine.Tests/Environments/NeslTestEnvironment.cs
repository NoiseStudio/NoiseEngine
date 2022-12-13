using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Nesl;
using System;

namespace NoiseEngine.Tests.Environments;

public abstract class NeslTestEnvironment : GraphicsTestEnvironment {

    protected NeslTestEnvironment(ApplicationFixture fixture) : base(fixture) {
    }

    protected void AssertFloat32BufferOutput(Action<IlGenerator> emitFactory, params float[] expectedValues) {
        // Create shader.
        NeslTypeBuilder shaderType = TestEmitHelper.NewType();

        NeslFieldBuilder shaderOutput = shaderType.DefineField("Output", BuiltInTypes.Float32);
        shaderOutput.AddAttribute(UniformAttribute.Create());

        NeslMethodBuilder shaderMethod = shaderType.DefineMethod("Main");
        shaderMethod.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = shaderMethod.IlGenerator;

        emitFactory(il);

        // Run.
        float[] read = new float[expectedValues.Length];

        foreach (GraphicsDevice device in GraphicsDevices) {
            GraphicsHostBuffer<float> buffer =
               new GraphicsHostBuffer<float>(device, GraphicsBufferUsage.Storage, 1);

            ComputeShader shader = new ComputeShader(device, shaderType);
            shader.GetProperty(shaderOutput)!.SetBuffer(buffer);

            GraphicsCommandBuffer commandBuffer = Fixture.GetCommandBuffer(device);
            commandBuffer.Dispatch(shader.GetKernel(shaderMethod)!, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            buffer.GetData(read);
            Assert.Equal(expectedValues, read);
        }
    }

}
