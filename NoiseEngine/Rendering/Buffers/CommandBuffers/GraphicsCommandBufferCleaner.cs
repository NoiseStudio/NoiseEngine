using System.Collections.Concurrent;
using System.Threading;

namespace NoiseEngine.Rendering.Buffers.CommandBuffers;

internal static class GraphicsCommandBufferCleaner {

    private const ulong Timeout = 1000000000;
    private const uint ObtainedTimeoutsWarningCount = 16;

    private static readonly ConcurrentQueue<GraphicsCommandBufferCleanData> queue =
        new ConcurrentQueue<GraphicsCommandBufferCleanData>();
    private static readonly AutoResetEvent resetEvent = new AutoResetEvent(false);

    static GraphicsCommandBufferCleaner() {
        new Thread(ThreadWorker) {
            IsBackground = true,
            Priority = ThreadPriority.Lowest,
            Name = nameof(GraphicsCommandBufferCleaner)
        }.Start();
    }

    public static void Enqueue(GraphicsCommandBufferCleanData data) {
        queue.Enqueue(data);
        resetEvent.Set();
    }

    private static void ThreadWorker() {
        while (true) {
            while (queue.TryDequeue(out GraphicsCommandBufferCleanData data)) {
                if (!GraphicsFence.WaitAll(data.Fences, Timeout)) {
                    data = data with { ObtainedTimeouts = data.ObtainedTimeouts + 1 };

                    if (data.ObtainedTimeouts >= ObtainedTimeoutsWarningCount) {
                        Log.Warning(
                            $"{nameof(GraphicsCommandBuffer)} {{ Handle = {data.Handle} }} lives too long. " +
                            $"Obtained {data.ObtainedTimeouts} timeouts in {nameof(GraphicsCommandBufferCleaner)}."
                        );
                    }

                    queue.Enqueue(data);
                    continue;
                }

                data.References.Clear();
                data.Fences.Clear();

                GraphicsCommandBuffer.DestroyHandle(data.Handle);
            }

            resetEvent.WaitOne();
        }
    }

}
