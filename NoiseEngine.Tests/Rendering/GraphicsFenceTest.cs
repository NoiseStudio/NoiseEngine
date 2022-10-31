using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using System.Linq;

namespace NoiseEngine.Tests.Rendering;

[Collection(nameof(ApplicationCollection))]
public class GraphicsFenceTest {

    private readonly GraphicsCommandBuffer[][] commandBuffers;

    public GraphicsFenceTest() {
        commandBuffers = Application.GraphicsInstance.Devices.Select(x => Enumerable.Range(0, 4).Select(
            _ => new GraphicsCommandBuffer(x, true)).ToArray()
        ).ToArray();
    }

    [Fact]
    public void IsSignaled() {
        foreach (GraphicsCommandBuffer[] c in commandBuffers) {
            GraphicsCommandBuffer commandBuffer = c[0];

            GraphicsFence fence = commandBuffer.Execute();
            _ = fence.IsSignaled;

            commandBuffer.Clear();

            Assert.True(fence.IsSignaled);
        }
    }

    [Fact]
    public void Wait() {
        foreach (GraphicsCommandBuffer[] c in commandBuffers) {
            GraphicsCommandBuffer commandBuffer = c[0];

            commandBuffer.Execute().Wait();
            commandBuffer.Clear();
        }
    }

    [Fact]
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

    [Fact]
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
