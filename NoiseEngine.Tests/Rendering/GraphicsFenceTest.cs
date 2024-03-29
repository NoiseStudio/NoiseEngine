﻿using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System.Linq;

namespace NoiseEngine.Tests.Rendering;

public class GraphicsFenceTest : GraphicsTestEnvironment {

    private readonly GraphicsCommandBuffer[][] commandBuffers;

    public GraphicsFenceTest(ApplicationFixture fixture) : base(fixture) {
        commandBuffers = Fixture.GraphicsDevices.Select(x => Enumerable.Range(0, 4).Select(
            _ => new GraphicsCommandBuffer(x, true)).ToArray()
        ).ToArray();
    }

    [FactRequire(TestRequirements.Graphics)]
    public void IsSignaled() {
        foreach (GraphicsCommandBuffer[] c in commandBuffers) {
            GraphicsCommandBuffer commandBuffer = c[0];

            GraphicsFence fence = commandBuffer.Execute();
            _ = fence.IsSignaled;

            commandBuffer.Clear();

            Assert.True(fence.IsSignaled);
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void Wait() {
        foreach (GraphicsCommandBuffer[] c in commandBuffers) {
            GraphicsCommandBuffer commandBuffer = c[0];

            commandBuffer.Execute().Wait();
            commandBuffer.Clear();
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void WaitAll() {
        foreach (GraphicsCommandBuffer[] c in commandBuffers) {
            GraphicsFence[] fences = new GraphicsFence[c.Length];

            int i = 0;
            foreach (GraphicsCommandBuffer commandBuffer in c)
                fences[i++] = commandBuffer.Execute();

            GraphicsFence.WaitAll(fences);

            foreach (GraphicsCommandBuffer commandBuffer in c)
                commandBuffer.Clear();
        }
    }

    [FactRequire(TestRequirements.Graphics)]
    public void WaitAny() {
        foreach (GraphicsCommandBuffer[] c in commandBuffers) {
            GraphicsFence[] fences = new GraphicsFence[c.Length];

            int i = 0;
            foreach (GraphicsCommandBuffer commandBuffer in c)
                fences[i++] = commandBuffer.Execute();

            GraphicsFence.WaitAny(fences);

            foreach (GraphicsCommandBuffer commandBuffer in c)
                commandBuffer.Clear();
        }
    }

}
