using NoiseEngine.Mathematics;
using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Linq;

namespace NoiseEngine.Tests.Environments;

public abstract class NeslExecuteTestEnvironment : NeslTestEnvironment {

    protected NeslExecuteTestEnvironment(ApplicationFixture fixture) : base(fixture) {
    }

    private protected void RunKernelWithSingleBuffer<T>(
        string code, T[]? initialValues, params T[] expectedValues
    ) where T : unmanaged {
        NeslAssembly assembly = NeslCompiler.Compile(
            nameof(RunKernelWithSingleBuffer), "/Abc/", new NeslFile[] { new NeslFile("/Abc/Example", code) }
        );

        NeslAttribute a = KernelAttribute.Create(1, 1, 1);
        NeslMethod kernel = assembly.Types.SelectMany(x => x.Methods)
            .Single(x => x.Attributes.HasAnyAttribute(a.FullName));
        NeslField field = kernel.Type.Fields.Single(x => x.IsUniform);

        T[] read = new T[expectedValues.Length];
        foreach (GraphicsDevice graphicsDevice in GraphicsDevices) {
            GraphicsHostBuffer<T> buffer = new GraphicsHostBuffer<T>(
                graphicsDevice, GraphicsBufferUsage.TransferAll | GraphicsBufferUsage.Storage,
                (ulong)Math.Max(initialValues?.Length ?? 0, expectedValues.Length)
            );

            if (initialValues is not null)
                buffer.SetData(initialValues);

            ComputeMaterial material = new ComputeMaterial(new ComputeShader(graphicsDevice, kernel.Type));
            material.GetProperty(field!)!.SetBuffer(buffer);

            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(graphicsDevice, false);
            commandBuffer.Dispatch(material.GetKernel(kernel!)!, material, Vector3<uint>.One);
            commandBuffer.Execute();
            commandBuffer.Clear();

            // Assert.
            buffer.GetData(read);
            Assert.Equal(expectedValues, read);
        }
    }

}
