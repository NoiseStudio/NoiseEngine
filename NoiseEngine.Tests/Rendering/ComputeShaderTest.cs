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

    private const float ValueAA = 18.64f;
    private const float ValueAB = 1324f;
    private const float ValueBA = 21.07f;
    private const float ValueBB = 2022f;

    private NeslTypeBuilder ShaderType { get; }
    private NeslFieldBuilder ShaderBufferA { get; }
    private NeslFieldBuilder ShaderBufferB { get; }
    private NeslMethodBuilder ShaderMethodA { get; }
    private NeslMethodBuilder ShaderMethodB { get; }

    public ComputeShaderTest(ApplicationFixture fixture) : base(fixture) {
        // Create shader.
        ShaderType = TestEmitHelper.NewType();

        ShaderBufferA = ShaderType.DefineField("bufferA", BuiltInTypes.Float32);
        ShaderBufferA.AddAttribute(UniformAttribute.Create());
        ShaderBufferB = ShaderType.DefineField("bufferB", BuiltInTypes.Float32);
        ShaderBufferB.AddAttribute(UniformAttribute.Create());

        // Method A.
        ShaderMethodA = ShaderType.DefineMethod("TestA");
        ShaderMethodA.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = ShaderMethodA.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 0u, ValueAA);
        il.Emit(OpCode.LoadFloat32, 1u, ValueAB);
        il.Emit(OpCode.Return);

        // Method B.
        ShaderMethodB = ShaderType.DefineMethod("TestB");
        ShaderMethodB.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        il = ShaderMethodB.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 0u, ValueBA);
        il.Emit(OpCode.LoadFloat32, 1u, ValueBB);
        il.Emit(OpCode.Return);
    }

    [Fact]
    public void ParrarelExecution() {
        float[] readData = new float[1];
        float[] readDataB = new float[1];

        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            // Create original shader.
            GraphicsHostBuffer<float> bufferAA = new GraphicsHostBuffer<float>(
                device, GraphicsBufferUsage.TransferSource | GraphicsBufferUsage.Storage, 1
            );
            GraphicsHostBuffer<float> bufferAB = new GraphicsHostBuffer<float>(
                device, GraphicsBufferUsage.TransferSource | GraphicsBufferUsage.Storage, 1
            );

            ComputeShader shader = new ComputeShader(device, ShaderType);
            ComputeMaterial materialA = new ComputeMaterial(shader);
            materialA.GetProperty(ShaderBufferA)!.SetBuffer(bufferAA);
            materialA.GetProperty(ShaderBufferB)!.SetBuffer(bufferAB);

            GraphicsCommandBuffer commandBuffer = Fixture.GetCommandBuffer(device);
            commandBuffer.Dispatch(shader.GetKernel(ShaderMethodA)!, materialA, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            bufferAA.GetData(readData);
            bufferAB.GetData(readDataB);
            Assert.Equal(new float[] { ValueAA }, readData);
            bufferAB.GetData(readData);
            Assert.Equal(new float[] { ValueAB }, readData);

            // Clone.
            ComputeMaterial materialB = new ComputeMaterial(shader);

            // Dispatch without setting properties.
            commandBuffer.Dispatch(shader.GetKernel(ShaderMethodA)!, materialA, Vector3<uint>.One);
            Assert.Throws<InvalidOperationException>(
                () => commandBuffer.Dispatch(shader.GetKernel(ShaderMethodB)!, materialB, Vector3<uint>.One)
            );
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            bufferAA.GetData(readData);
            Assert.Equal(new float[] { ValueAA }, readData);
            bufferAB.GetData(readData);
            Assert.Equal(new float[] { ValueAB }, readData);

            // Dispatch with setting properties.
            GraphicsHostBuffer<float> bufferBA = new GraphicsHostBuffer<float>(
                device, GraphicsBufferUsage.TransferSource | GraphicsBufferUsage.Storage, 1
            );
            GraphicsHostBuffer<float> bufferBB = new GraphicsHostBuffer<float>(
                device, GraphicsBufferUsage.TransferSource | GraphicsBufferUsage.Storage, 1
            );

            materialB.GetProperty(ShaderBufferA)!.SetBuffer(bufferBA);
            materialB.GetProperty(ShaderBufferB)!.SetBuffer(bufferBB);

            commandBuffer.Dispatch(shader.GetKernel(ShaderMethodA)!, materialA, Vector3<uint>.One);
            commandBuffer.Dispatch(shader.GetKernel(ShaderMethodB)!, materialB, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            bufferAA.GetData(readData);
            Assert.Equal(new float[] { ValueAA }, readData);
            bufferAB.GetData(readData);
            Assert.Equal(new float[] { ValueAB }, readData);
            bufferBA.GetData(readData);
            Assert.Equal(new float[] { ValueBA }, readData);
            bufferBB.GetData(readData);
            Assert.Equal(new float[] { ValueBB }, readData);
        }
    }

}
