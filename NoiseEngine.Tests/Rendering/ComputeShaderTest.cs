using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using NoiseEngine.Tests.Nesl;
using System;

namespace NoiseEngine.Tests.Rendering;

public class ComputeShaderTest : GraphicsTestEnvironment {

    private const float Value = 18.64f;

    private NeslTypeBuilder ShaderType { get; }
    private NeslFieldBuilder ShaderBuffer { get; }
    private NeslMethodBuilder ShaderMethod { get; }

    public ComputeShaderTest(ApplicationFixture fixture) : base(fixture) {
        // Create shader.
        ShaderType = TestEmitHelper.NewType();

        ShaderBuffer = ShaderType.DefineField("buffer", BuiltInTypes.Float32);
        ShaderBuffer.AddAttribute(StaticAttribute.Create());

        ShaderMethod = ShaderType.DefineMethod("Main");
        IlGenerator il = ShaderMethod.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 0u, Value);
        il.Emit(OpCode.Return);
    }

    [Fact]
    public void Clone() {
        float[] readData = new float[1];

        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            // Create original shader.
            GraphicsHostBuffer<float> bufferA =
               new GraphicsHostBuffer<float>(device, GraphicsBufferUsage.Storage, 1);

            ComputeShader shaderA = new ComputeShader(device, ShaderType);
            shaderA.GetProperty(ShaderBuffer)!.SetBuffer(bufferA);

            GraphicsCommandBuffer commandBuffer = Fixture.GetCommandBuffer(device);
            commandBuffer.Dispatch(shaderA.GetKernel(ShaderMethod)!, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            bufferA.GetData(readData);
            Assert.Equal(new float[] { Value }, readData);

            // Clone.
            ComputeShader shaderB = shaderA.Clone();

            // Dispatch without setting properties.
            commandBuffer.Dispatch(shaderA.GetKernel(ShaderMethod)!, Vector3<uint>.One);
            Assert.Throws<InvalidOperationException>(
                () => commandBuffer.Dispatch(shaderB.GetKernel(ShaderMethod)!, Vector3<uint>.One)
            );
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            bufferA.GetData(readData);
            Assert.Equal(new float[] { Value }, readData);

            // Dispatch with setting properties.
            GraphicsHostBuffer<float> bufferB =
                new GraphicsHostBuffer<float>(device, GraphicsBufferUsage.Storage, 1);
            shaderB.GetProperty(ShaderBuffer)!.SetBuffer(bufferB);

            commandBuffer.Dispatch(shaderA.GetKernel(ShaderMethod)!, Vector3<uint>.One);
            commandBuffer.Dispatch(shaderB.GetKernel(ShaderMethod)!, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            bufferA.GetData(readData);
            Assert.Equal(new float[] { Value }, readData);
            bufferB.GetData(readData);
            Assert.Equal(new float[] { Value }, readData);
        }
    }

}
