using NoiseEngine.Rendering.Buffers;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NoiseEngine;

public sealed class PerformanceRenderLoop : RenderLoop {

    private readonly object framesInFlightLocker = new object();
    private readonly ConcurrentStack<GraphicsCommandBuffer> commandBuffers =
        new ConcurrentStack<GraphicsCommandBuffer>();

    private AutoResetEvent? executeFrameResetEvent;
    private AutoResetEvent? signalResetEvent;
    private GraphicsCommandBuffer? currentCommandBuffer;
    private bool executeFrameThreadWork;
    private uint rendererSignaler;

    private uint framesInFlight = 1;
    private uint? executionThreadCount = 16;

    public uint FramesInFlight {
        get => framesInFlight;
        set {
            if (value == 0) {
                throw new ArgumentOutOfRangeException(
                    nameof(FramesInFlight), $"Minimum value of {nameof(FramesInFlight)} is one."
                );
            }

            ChangeFramesInFlightCount(value);
        }
    }

    public uint? ExecutionThreadCount {
        get => executionThreadCount;
        set {
            if (value == 0) {
                throw new ArgumentOutOfRangeException(
                    nameof(ExecutionThreadCount), $"Minimum value of {nameof(ExecutionThreadCount)} is one."
                );
            }

            executionThreadCount = value;
        }
    }

    /// <summary>
    /// Calls on <see cref="RenderLoop"/> initialization.
    /// </summary>
    protected override void Initialize() {
        executeFrameThreadWork = true;
        rendererSignaler = 1;

        signalResetEvent = new AutoResetEvent(true);
        executeFrameResetEvent = new AutoResetEvent(false);

        new Thread(RenderWorker) {
            IsBackground = true,
            Priority = ThreadPriority.Highest,
            Name = $"{ToString()} Renderer"
        }.Start();

        ChangeFramesInFlightCount(FramesInFlight);
    }

    /// <summary>
    /// Calls on <see cref="RenderLoop"/> is deinitialization.
    /// </summary>
    protected override void Deinitialize() {
        throw new NotImplementedException();
    }

    private void ChangeFramesInFlightCount(uint targetFramesInFlightCount) {
        lock (framesInFlightLocker) {
            if (Camera is null) {
                framesInFlight = targetFramesInFlightCount;
                return;
            }

            framesInFlight = Camera.Delegation.ChangeFramesInFlightCount(targetFramesInFlightCount);

            uint threadCount = (ExecutionThreadCount.HasValue ? ExecutionThreadCount.Value : framesInFlight) + 1;
            for (int i = 1; i < threadCount; i++) {
                new Thread(ExecuteFrameWorker) {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal,
                    Name = $"{ToString()} Executor #{i}"
                }.Start();
            }
        }
    }

    private void RenderWorker() {
        Camera camera = Camera ?? throw new NullReferenceException();
        Window window = Window ?? throw new NullReferenceException();
        ConcurrentStack<GraphicsCommandBuffer> commandBuffers = this.commandBuffers;
        AutoResetEvent executeFrameResetEvent = this.executeFrameResetEvent!;
        AutoResetEvent signalResetEvent = this.signalResetEvent!;

        while (!window.IsDisposed) {
            //WindowInterop.PoolEvents(window.Handle);
            //signalResetEvent.WaitOne();
            while (rendererSignaler == 0)
                Thread.Sleep(0);
            Interlocked.Decrement(ref rendererSignaler);

            if (!commandBuffers.TryPop(out GraphicsCommandBuffer? commandBuffer))
                commandBuffer = new GraphicsCommandBuffer(camera.GraphicsDevice, false);

            commandBuffer.AttachCameraUnchecked(camera);
            commandBuffer.DetachCameraUnchecked();

            GraphicsCommandBuffer? exchanged;
            do {
                exchanged = Interlocked.CompareExchange(ref currentCommandBuffer, commandBuffer, null);
            } while (exchanged is not null);

            executeFrameResetEvent.Set();
        }

        executeFrameThreadWork = false;
    }

    private void ExecuteFrameWorker() {
        ConcurrentStack<GraphicsCommandBuffer> commandBuffers = this.commandBuffers;
        AutoResetEvent executeFrameResetEvent = this.executeFrameResetEvent!;
        AutoResetEvent signalResetEvent = this.signalResetEvent!;

        while (executeFrameThreadWork) {
            executeFrameResetEvent.WaitOne();
            Interlocked.Increment(ref rendererSignaler);
            //signalResetEvent.Set();

            GraphicsCommandBuffer? current = Interlocked.Exchange(ref currentCommandBuffer, null);
            if (current is null) {
                if (!executeFrameThreadWork)
                    throw new InvalidOperationException("Frame was omitted.");
                else
                    break;
            }

            current.Execute();
            current.Clear();
            commandBuffers.Push(current);
        }
    }

}
