﻿using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Rendering;
using NoiseEngine.Systems;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NoiseEngine;

public sealed class PerformanceRenderLoop : RenderLoop {

    private readonly object updateLocker = new object();
    private readonly ConcurrentStack<RenderFrameResources> frameResources =
        new ConcurrentStack<RenderFrameResources>();

    private bool renderThreadWork;
    private bool[] executeThreadWork = Array.Empty<bool>();
    private uint executeWorkingThreadCount;
    private AutoResetEvent? rendererResetEvent;
    private uint rendererSignaler;
    private AutoResetEvent? executeResetEvent;
    private RenderFrameResources? currentFrame;
    private AutoResetEvent? deinitializeResetEvent;
    private bool isDeinitialized;

    private uint framesInFlight = 3;
    private uint? executionThreadCount;

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

            ChangeExecutionThreadCount(value);
        }
    }

    /// <summary>
    /// Calls on <see cref="RenderLoop"/> initialization.
    /// </summary>
    protected override void Initialize() {
        isDeinitialized = false;

        rendererResetEvent = new AutoResetEvent(true);
        executeResetEvent = new AutoResetEvent(false);

        renderThreadWork = true;
        rendererSignaler = 1;

        new Thread(RenderWorker) {
            IsBackground = false,
            Priority = ThreadPriority.Highest,
            Name = $"{ToString()} Renderer"
        }.Start();

        ChangeFramesInFlightCount(FramesInFlight);
        ChangeExecutionThreadCount(ExecutionThreadCount);
    }

    /// <summary>
    /// Calls on <see cref="RenderLoop"/> is deinitialization.
    /// </summary>
    protected override void Deinitialize() {
        lock (updateLocker) {
            if (isDeinitialized)
                return;
            deinitializeResetEvent = new AutoResetEvent(false);
        }

        renderThreadWork = false;

        deinitializeResetEvent.WaitOne();
        deinitializeResetEvent.Dispose();
        deinitializeResetEvent = null;
    }

    private void ChangeFramesInFlightCount(uint targetFramesInFlightCount) {
        lock (updateLocker) {
            if (Camera is null) {
                framesInFlight = targetFramesInFlightCount;
                return;
            }

            framesInFlight = Camera.Delegation.ChangeFramesInFlightCount(targetFramesInFlightCount);

            if (!ExecutionThreadCount.HasValue)
                ChangeExecutionThreadCount(ExecutionThreadCount);
        }
    }

    private void ChangeExecutionThreadCount(uint? newExecutionThreadCount) {
        lock (updateLocker) {
            executionThreadCount = newExecutionThreadCount;
            if (!renderThreadWork)
                return;

            uint threadCount = ExecutionThreadCount.HasValue ? ExecutionThreadCount.Value : Math.Min(
                framesInFlight, (uint)Environment.ProcessorCount
            );

            if (threadCount > executeThreadWork.Length)
                Array.Resize(ref executeThreadWork, (int)threadCount);

            for (int i = 0; i < threadCount; i++) {
                if (executeThreadWork[i])
                    continue;

                executeThreadWork[i] = true;
                new Thread(ExecuteWorker) {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal,
                    Name = $"{ToString()} Executor #{i + 1}"
                }.Start(i);
            }

            for (int i = (int)threadCount; i < executeThreadWork.Length; i++)
                executeThreadWork[i] = false;
        }
    }

    private void RenderWorker() {
        Camera camera = Camera ?? throw new NullReferenceException();
        Window window = Window ?? throw new NullReferenceException();
        AutoResetEvent rendererResetEvent = this.rendererResetEvent ?? throw new NullReferenceException();
        AutoResetEvent executeResetEvent = this.executeResetEvent ?? throw new NullReferenceException();
        ConcurrentStack<RenderFrameResources> frameResources = this.frameResources;
        object pollEventsLocker = window.PollEventsLocker;
        ConcurrentList<EntitySystem> frameDependentSystems = camera.Scene.FrameDependentSystems;

        MeshRendererSystem meshRendererSystem = new MeshRendererSystem(camera);
        camera.Scene.AddSystem(meshRendererSystem);

        try {
            RenderFrameResources? frame;
            RenderFrameResources? exchanged;
            TransformComponent transform;

            while (renderThreadWork) {
                lock (pollEventsLocker) {
                    window.PollEvents();

                    foreach (EntitySystem system in frameDependentSystems)
                        system.TryExecute();

                    if (rendererSignaler == 0)
                        rendererResetEvent.WaitOne();
                    Interlocked.Decrement(ref rendererSignaler);

                    if (!frameResources.TryPop(out frame))
                        frame = new RenderFrameResources(camera.GraphicsDevice, camera);

                    foreach (EntitySystem system in frameDependentSystems)
                        system.Wait();
                }

                if (camera.Entity.TryGet(out transform)) {
                    camera.Position = transform.Position;
                    camera.Rotation = transform.Rotation;
                }

                meshRendererSystem.Resources = frame.MeshRendererResources;
                meshRendererSystem.ExecuteAndWait();

                do {
                    exchanged = Interlocked.CompareExchange(ref currentFrame, frame, null);
                } while (exchanged is not null);

                executeResetEvent.Set();
            }
        } catch (Exception exception) {
            Log.Error($"Thread terminated due to an exception. {exception}");
            throw;
        } finally {
            for (int i = 0; i < executeThreadWork.Length; i++)
                executeThreadWork[i] = false;

            RenderFrameResources? exchanged = Interlocked.Exchange(ref currentFrame, null);
            if (exchanged is not null)
                exchanged.Clear();

            while (executeWorkingThreadCount != 0) {
                for (int i = 0; i < executeThreadWork.Length; i++)
                    executeResetEvent.Set();
                Thread.Sleep(1);
            }

            rendererResetEvent.Dispose();
            this.rendererResetEvent = null;
            executeResetEvent.Dispose();
            this.executeResetEvent = null;

            lock (updateLocker) {
                deinitializeResetEvent?.Set();
                isDeinitialized = true;
            }
        }
    }

    private void ExecuteWorker(object? idObject) {
        int id = (int)(idObject ?? throw new NullReferenceException());
        AutoResetEvent rendererResetEvent = this.rendererResetEvent ?? throw new NullReferenceException();
        AutoResetEvent executeResetEvent = this.executeResetEvent ?? throw new NullReferenceException();
        ConcurrentStack<RenderFrameResources> frameResources = this.frameResources;

        Interlocked.Increment(ref executeWorkingThreadCount);

        try {
            RenderFrameResources? current;

            while (executeThreadWork[id]) {
                executeResetEvent.WaitOne();
                Interlocked.Increment(ref rendererSignaler);
                rendererResetEvent.Set();

                current = Interlocked.Exchange(ref currentFrame, null);
                if (current is null) {
                    if (executeThreadWork[id])
                        throw new InvalidOperationException("Frame was omitted.");
                    else
                        break;
                }

                current.RecordAndExecute();
                current.Clear();

                frameResources.Push(current);
            }
        } catch (Exception exception) {
            Log.Error($"Thread terminated due to an exception. {exception}");
            throw;
        } finally {
            Interlocked.Decrement(ref executeWorkingThreadCount);
        }
    }

}
