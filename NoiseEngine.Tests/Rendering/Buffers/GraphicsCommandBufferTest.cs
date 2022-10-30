using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using System.Linq;
using System.Threading;

namespace NoiseEngine.Tests.Rendering.Buffers;

[Collection(nameof(ApplicationCollection))]
public class GraphicsCommandBufferTest {

    [Fact]
    public void CopyBuffer() {
        const ulong Size = 1024;

        int[] data = Enumerable.Range(0, (int)Size).ToArray();

        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
            GraphicsCommandBuffer commandBuffer = new GraphicsCommandBuffer(device);

            using GraphicsHostBuffer<int> sourceBuffer = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.TransferSource, Size
            );
            using GraphicsHostBuffer<int> destinationBuffer = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.TransferDestination, Size
            );

            sourceBuffer.SetData(data);

            commandBuffer.Copy(sourceBuffer, destinationBuffer, Size);
            commandBuffer.Execute();

            int[] read = new int[Size];
            destinationBuffer.GetData(read);

            Assert.Equal(data, read);
        }
    }

}
