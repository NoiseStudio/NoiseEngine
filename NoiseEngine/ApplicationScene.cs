using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Systems;
using NoiseEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NoiseEngine {
    public class ApplicationScene : IDisposable {

        private readonly ConcurrentHashSet<RenderCamera> cameras = new ConcurrentHashSet<RenderCamera>();

        private AtomicBool isDisposed;

        public EntityWorld EntityWorld { get; } = new EntityWorld();
        public Application Application { get; }
        public PrimitiveCreator Primitive { get; }

        public bool IsDisposed => isDisposed;
        public IEnumerable<RenderCamera> Cameras => cameras;

        internal ConcurrentBag<EntitySystemBase> FrameDependentSystems { get; } = new ConcurrentBag<EntitySystemBase>();

        public ApplicationScene(Application application) {
            Application = application;
            Primitive = new PrimitiveCreator(this);

            Application.AddSceneToLoaded(this);
        }

        public RenderCamera CreateWindow(
            string? title = null, uint width = 1280, uint height = 720, bool autoRender = true
        ) {
            title ??= Application.ApplicationName;

            RenderCamera camera = new RenderCamera(
                this,
                new Camera(new Window(Application.GraphicsDevice, new UInt2(width, height), title)),
                autoRender
            );
            cameras.Add(camera);

            if (!HasFrameDependentSystem<CameraSystem>())
                AddFrameDependentSystem(new CameraSystem());

            Interlocked.CompareExchange(ref Application.mainWindow, camera, null);
            return camera;
        }

        /// <summary>
        /// Initializes and adds <paramref name="system"/> to systems witch will be executed on each render frame.
        /// </summary>
        /// <param name="system"><see cref="EntitySystemBase"/> system witch will be
        /// executed on each render frame.</param>
        public void AddFrameDependentSystem(EntitySystemBase system) {
            system.Initialize(EntityWorld, Application.EntitySchedule);
            FrameDependentSystems.Add(system);
        }

        public bool HasFrameDependentSystem<T>() where T : EntitySystemBase {
            Type type = typeof(T);
            return FrameDependentSystems.Any(x => x.GetType() == type);
        }

        public void Dispose() {
            if (isDisposed.Exchange(true))
                return;

            Application.RemoveSceneFromLoaded(this);

            OnDispose();

            FrameDependentSystems.Clear();
            EntityWorld.Dispose();
            Primitive.Dispose();
        }

        internal void RemoveRenderCameraFromScene(RenderCamera renderCamera) {
            cameras.Remove(renderCamera);
        }

        protected virtual void OnDispose() {
        }

    }
}
