using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Rendering.Buffers;

public class GraphicsCommandBufferTest : GraphicsTestEnvironment {

    private const ulong Size = 1024;

    private readonly GraphicsCommandBuffer[] commandBuffer;

    private readonly Random random = new Random();
    private readonly int[] readInt = new int[1024];

    private readonly GraphicsHostBuffer<int>[] hostBufferA;
    private readonly GraphicsHostBuffer<int>[] hostBufferB;

    public GraphicsCommandBufferTest(ApplicationFixture fixture) : base(fixture) {
        int length = Fixture.GraphicsDevices.Count;

        commandBuffer = new GraphicsCommandBuffer[length];

        hostBufferA = new GraphicsHostBuffer<int>[length];
        hostBufferB = new GraphicsHostBuffer<int>[length];

        int i = 0;
        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            commandBuffer[i] = new GraphicsCommandBuffer(device, true);

            hostBufferA[i] = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.TransferAll | GraphicsBufferUsage.Uniform, Size
            );
            hostBufferB[i] = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.TransferAll, Size
            );

            i++;
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void CopyBuffer() {
        int i = 0;
        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            int[] data = GetRandomData();
            hostBufferA[i].SetData(data);

            commandBuffer[i].Copy(hostBufferA[i], hostBufferB[i], Size);
            commandBuffer[i].Execute();
            commandBuffer[i].Clear();

            hostBufferB[i].GetData(readInt);
            Assert.Equal(data, readInt);

            i++;
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void Dispatch() {
        const float Value = 18.64f;

        // Shader code.
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(Dispatch));

        NeslTypeBuilder shaderType = assembly.DefineType("Shader");

        NeslFieldBuilder buffer = shaderType.DefineField("buffer", BuiltInTypes.Float32);
        buffer.AddAttribute(UniformAttribute.Create());

        NeslMethodBuilder main = shaderType.DefineMethod("Main");
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 0u, Value);
        il.Emit(OpCode.Return);

        // Create compute shader.
        float[] readData = new float[1];

        int i = 0;
        foreach (GraphicsDevice device in Fixture.GraphicsDevices) {
            GraphicsHostBuffer<float> hostBuffer =
                new GraphicsHostBuffer<float>(device, GraphicsBufferUsage.TransferSource, 1);

            ComputeShader shader = new ComputeShader(device, shaderType);
            ComputeMaterial material = new ComputeMaterial(shader);
            material.GetProperty(buffer)!.SetBuffer(hostBuffer);

            // Dispatch.
            commandBuffer[i].Dispatch(shader.GetKernel(main)!, material, Vector3<uint>.One);
            commandBuffer[i].Execute();
            commandBuffer[i].Clear();

            // Assert.
            hostBuffer.GetData(readData);
            Assert.Equal(new float[] { Value }, readData);

            i++;
        }
    }

    private int[] GetRandomData(ulong size = Size) {
        int[] data = new int[size];
        for (int i = 0; i < data.Length; i++)
            data[i] = random.Next();
        return data;
    }

}
