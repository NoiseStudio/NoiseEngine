using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan;
using NoiseEngine.Rendering.Vulkan.Descriptors;
using NoiseEngine.Tests.Fixtures;
using System;
using System.IO;
using System.Runtime.InteropServices;

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
    public unsafe void Dispatch() {
        // Shader.
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(Dispatch));

        NeslTypeBuilder shader = assembly.DefineType("Shader");

        NeslFieldBuilder buffer = shader.DefineField("buffer", BuiltInTypes.Float32);
        buffer.AddAttribute(StaticAttribute.Create());

        NeslMethodBuilder main = shader.DefineMethod("Main");
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.LoadFloat32, 0u, 18.64f);
        il.Emit(OpCode.Return);

        SpirVCompilationResult result = SpirVCompiler.Compile(new NeslEntryPoint[] {
            new NeslEntryPoint(main, ExecutionModel.GLCompute)
        });

        // Descriptor set data.
        ReadOnlySpan<DescriptorSetLayoutBinding> bindings = stackalloc DescriptorSetLayoutBinding[] {
            new DescriptorSetLayoutBinding(0, DescriptorType.Storage, 1, ShaderStageFlags.Compute, 0)
        };
        ReadOnlySpan<DescriptorUpdateTemplateEntry> entries = stackalloc DescriptorUpdateTemplateEntry[] {
            new DescriptorUpdateTemplateEntry(0, 0, 1, DescriptorType.Storage, 0, 0)
        };

        ReadOnlySpan<byte> data = stackalloc byte[Marshal.SizeOf<DescriptorBufferInfo>()];
        float[] readData = new float[1];

        int i = 0;
        foreach (GraphicsDevice d in Application.GraphicsInstance.Devices) {
            if (d is not VulkanDevice device)
                continue;

            // Create host buffer.
            GraphicsHostBuffer<float> hostBuffer =
                new GraphicsHostBuffer<float>(device, GraphicsBufferUsage.Storage, 1);

            // Create descriptor set.
            DescriptorSetLayout layout = new DescriptorSetLayout(device, bindings);
            DescriptorSet set = new DescriptorSet(layout);
            DescriptorUpdateTemplate template = new DescriptorUpdateTemplate(layout, entries);

            fixed (byte* pointer = data)
                Marshal.StructureToPtr(DescriptorBufferInfo.Create(hostBuffer), (nint)pointer, false);

            set.Update(template, data);

            // Create shader module.
            ShaderModule module = new ShaderModule(device, result.GetCode());

            // Create pipeline.
            PipelineLayout pipelineLayout = new PipelineLayout(new DescriptorSetLayout[] { layout });
            ComputePipeline pipeline = new ComputePipeline(
                pipelineLayout, new PipelineShaderStage(ShaderStageFlags.Compute, module, new Guid().ToString()),
                PipelineCreateFlags.None
            );

            // Dispatch.
            commandBuffer[i].DispatchUnchecked(pipeline, set, Vector3<uint>.One);
            commandBuffer[i].Execute();
            commandBuffer[i].Clear();

            // Assert.
            hostBuffer.GetData(readData);
            Assert.Equal(new float[] { 18.64f }, readData);

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
