using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine;

public class ApplicationScene : EntityWorld {

    private readonly ConcurrentList<Camera> cameras = new ConcurrentList<Camera>();

    private GraphicsDevice? graphicsDevice;
    private PrimitiveCreator? primitive;

    public GraphicsDevice GraphicsDevice {
        get {
            if (graphicsDevice is null)
                graphicsDevice = Application.GraphicsInstance.Devices.First();
            return graphicsDevice!;
        }
        init => graphicsDevice = value;
    }

    public PrimitiveCreator Primitive {
        get {
            if (primitive is null)
                Interlocked.CompareExchange(ref primitive, new PrimitiveCreator(this), null);
            return primitive;
        }
    }

    public IEnumerable<Camera> Cameras => cameras;

    internal ConcurrentList<EntitySystem> FrameDependentSystems { get; } = new ConcurrentList<EntitySystem>();

    public ApplicationScene() {
        Application.AddSceneToLoaded(this);
    }

    /// <summary>
    /// Initializes and adds <paramref name="system"/> to systems witch will be executed on each render frame.
    /// </summary>
    /// <param name="system"><see cref="EntitySystem"/> system witch will be
    /// executed on each render frame.</param>
    public void AddFrameDependentSystem(EntitySystem system) {
        AddSystem(system);
        FrameDependentSystems.Add(system);
    }

    /// <summary>
    /// Checks if this <see cref="ApplicationScene"/> has frame dependent system of a T type.
    /// </summary>
    /// <typeparam name="T">Type of <see cref="EntitySystem"/>.</typeparam>
    /// <returns><see langword="true"/> when this <see cref="ApplicationScene"/> has frame dependent system
    /// of a T type; otherwise <see langword="false"/>.</returns>
    public bool HasFrameDependentSystem<T>() where T : EntitySystem {
        return FrameDependentSystems.Any(x => x.GetType() == typeof(T));
    }

    private protected override void OnDisposeInternal() {
        Application.RemoveSceneFromLoaded(this);

        foreach (Camera camera in Cameras)
            camera.RenderTarget = null;

        FrameDependentSystems.Clear();

        GC.SuppressFinalize(this);
    }

    internal void AddCameraToScene(Camera camera) {
        cameras.Add(camera);
    }

    internal void RemoveCameraFromScene(Camera camera) {
        cameras.Remove(camera);
    }

}
