using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Rendering.Buffers;

[Collection(nameof(ApplicationCollection))]
public class GraphicsCommandBufferTest {

    private const ulong Size = 1024;

    private readonly GraphicsCommandBuffer[] commandBuffer;

    private readonly Random random = new Random();
    private readonly int[] readInt = new int[1024];

    private readonly GraphicsHostBuffer<int>[] hostBufferA;
    private readonly GraphicsHostBuffer<int>[] hostBufferB;

    public GraphicsCommandBufferTest() {
        int length = Application.GraphicsInstance.Devices.Count;

        commandBuffer = new GraphicsCommandBuffer[length];

        hostBufferA = new GraphicsHostBuffer<int>[length];
        hostBufferB = new GraphicsHostBuffer<int>[length];

        int i = 0;
        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
            commandBuffer[i] = new GraphicsCommandBuffer(device, true);

            hostBufferA[i] = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.TransferSource | GraphicsBufferUsage.Uniform, Size
            );
            hostBufferB[i] = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.TransferDestination, Size
            );

            i++;
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void CopyBuffer() {
        int i = 0;
        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
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
        buffer.AddAttribute(StaticAttribute.Create());

        NeslMethodBuilder main = shaderType.DefineMethod("Main");
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 0u, Value);
        il.Emit(OpCode.Return);

        // Create compute shader.
        float[] readData = new float[1];

        int i = 0;
        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
            GraphicsHostBuffer<float> hostBuffer =
                new GraphicsHostBuffer<float>(device, GraphicsBufferUsage.Storage, 1);

            ComputeShader shader = new ComputeShader(device, shaderType);
            shader.GetProperty(buffer)!.SetBuffer(hostBuffer);

            // Dispatch.
            commandBuffer[i].Dispatch(shader.GetKernel(main)!, Vector3<uint>.One);
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
