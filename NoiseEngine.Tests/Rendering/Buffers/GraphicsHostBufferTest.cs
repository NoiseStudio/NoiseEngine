using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Tests.Fixtures;
using System.Linq;

namespace NoiseEngine.Tests.Rendering.Buffers;

[Collection(nameof(ApplicationCollection))]
public class GraphicsHostBufferTest {

    [FactRequire(TestRequirements.Graphics)]
    public void CreateDestroy() {
        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices)
            new GraphicsHostBuffer<int>(device, GraphicsBufferUsage.Storage, 1024).Dispose();
    }

    [FactRequire(TestRequirements.Graphics)]
    public void SetGetData() {
        const int Size = 1024;

        int[] data = Enumerable.Range(0, Size).ToArray();

        foreach (GraphicsDevice device in Application.GraphicsInstance.Devices) {
            int[] read = new int[Size];

            using GraphicsHostBuffer<int> buffer = new GraphicsHostBuffer<int>(
                device, GraphicsBufferUsage.Storage, Size
            );

            buffer.SetData(data);

            buffer.GetData(read);
            Assert.Equal(data, read);
        }
    }

}
