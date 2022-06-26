using NoiseEngine.Components;
using NoiseEngine.Jobs;
using NoiseEngine.Logging;
using NoiseEngine.Logging.Standard;
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
using System.Reflection;
using System.Threading;

namespace NoiseEngine {
    public class Application : IDisposable {

        private readonly ManualResetEvent applicationEndEvent = new ManualResetEvent(true);
        private readonly object sceneLocker = new object();

        private ConcurrentBag<CameraData> cameraData = new ConcurrentBag<CameraData>();

        private uint activeRenderers;
        private AtomicBool isDisposed;
        private ApplicationScene currentScene;
        private bool simpleCreated;

        public Window? MainWindow { get; set; }
        public EntitySchedule EntitySchedule { get; }
        public string Title { get; }
        public Logger Logger { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public PrimitiveCreator Primitive { get; }

        public ApplicationScene CurrentScene => currentScene;
        public bool IsDisposed => isDisposed;
        public IEnumerable<Window> Windows => cameraData.Where(x => x.IsWindow).Select(x => x.RenderTarget);

        public Application(
            ApplicationScene scene, Logger logger, GraphicsDevice graphicsDevice,
            EntitySchedule entitySchedule, string title
        ) {
            Logger = logger;
            GraphicsDevice = graphicsDevice;
            Title = title;
            Primitive = new PrimitiveCreator(this);
            EntitySchedule = entitySchedule;

            currentScene = scene;
            CurrentScene.Initialize(this);
            CurrentScene.OnLoadInternal();

            Logger.Info($"Created application named: `{Title}`.");
        }

        /// <summary>
        /// Creates simple default <see cref="Application"/>.
        /// </summary>
        /// <param name="cameraEntity"><see cref="Entity"/> with <see cref="Camera"/>.</param>
        /// <param name="title">Title of <see cref="Application"/>.
        /// By default this will be the name of the entry assembly.</param>
        /// <param name="visibleLogs">Defines what logs will be processed by the handlers.</param>
        /// <returns>New instance of <see cref="Application"/>.</returns>
        public static Application Create(
            out CameraData cameraData, string? title = null, ApplicationScene? scene = null,
            LogType visibleLogs = LogType.AllWithoutTrace
        ) {
            scene ??= new DefaultApplicationScene();
            title ??= Assembly.GetEntryAssembly()?.GetName().Name!;

            Logger logger = new Logger(visibleLogs);
            logger.AddHandler(new ConsoleLoggerHandler(new LoggerConfiguration()));
            logger.AddHandler(FileLoggerHandler.CreateLogFileInDirectory("logs"));

            Graphics.Initialize(logger, title, Assembly.GetEntryAssembly()!.GetName().Version!);
            GraphicsDevice graphicsDevice = new GraphicsDevice(false);

            Application application = new Application(scene, logger, graphicsDevice, new EntitySchedule(), title);
            application.simpleCreated = true;

            if (!scene.HasFrameDependentSystem<CameraSystem>())
                scene.AddFrameDependentSystem(new CameraSystem());

            cameraData = application.CreateRenderCamera(new Window(graphicsDevice, new UInt2(1280, 720), title));
            return application;
        }

        /// <summary>
        /// Creates simple default <see cref="Application"/>.
        /// </summary>
        /// <param name="title">Title of <see cref="Application"/>.
        /// By default this will be the name of the entry assembly.</param>
        /// <param name="visibleLogs">Defines what logs will be processed by the handlers.</param>
        /// <returns>New instance of <see cref="Application"/>.</returns>
        public static Application Create(
            string? title = null, ApplicationScene? scene = null, LogType visibleLogs = LogType.AllWithoutTrace
        ) {
            return Create(out _, title, scene, visibleLogs);
        }

        /// <summary>
        /// Blocks the current thread until the <see cref="Application"/> ends work.
        /// </summary>
        public void WaitToEnd() {
            applicationEndEvent.WaitOne();
        }

        /// <summary>
        /// Creates new <see cref="Entity"/> with <see cref="Camera"/>.
        /// </summary>
        /// <param name="renderTarget">Object to which the <see cref="Camera"/> output image will be drawn.</param>
        /// <returns><see cref="Entity"/> with <see cref="Camera"/>.</returns>
        public CameraData CreateRenderCamera(Window renderTarget) {
            TransformComponent transform = new TransformComponent(new Float3(0, 0, -5));

            Camera camera = new Camera(renderTarget) {
                ProjectionType = ProjectionType.Perspective,
                NearClipPlane = 0.01f,
                FarClipPlane = 1000.0f,
                Position = transform.Position,
                Rotation = transform.Rotation
            };

            CameraData data = new CameraData(this, renderTarget, camera);
            cameraData.Add(data);

            Thread windowThread = new Thread(RenderThreadWorker) {
                Name = Title
            };
            windowThread.Start(data);

            data.UpdateEntity(CurrentScene.EntityWorld.NewEntity(transform, new CameraComponent(data)));
            return data;
        }

        public void LoadScene(ApplicationScene scene) {
            lock (sceneLocker) {
                ApplicationScene lastScene = Interlocked.Exchange(ref currentScene, scene);
                if (lastScene == scene)
                    return;

                lastScene.OnUnloadInternal();
                CurrentScene.Initialize(this);

                ConcurrentBag<CameraData> cameraData = new ConcurrentBag<CameraData>();
                foreach (CameraData data in this.cameraData) {
                    if (data.IsWindow && scene.ReuseWindowInternal(data, out Entity newCameraEntity)) {
                        cameraData.Add(data);

                        if (!newCameraEntity.Has<TransformComponent>(scene.EntityWorld))
                            newCameraEntity.Add(scene.EntityWorld, new TransformComponent(new Float3(0, 0, -5)));
                        if (!newCameraEntity.Has<CameraComponent>(scene.EntityWorld))
                            newCameraEntity.Add(scene.EntityWorld, new CameraComponent(data));

                        data.UpdateScene();
                        data.UpdateEntity(newCameraEntity);

                        continue;
                    }

                    data.Dispose();
                }

                this.cameraData = cameraData;
                scene.OnLoadInternal();

                if (!scene.HasFrameDependentSystem<CameraSystem>())
                    scene.AddFrameDependentSystem(new CameraSystem());
            }
        }

        /// <summary>
        /// Disposes this <see cref="Application"/>.
        /// </summary>
        public void Dispose() {
            if (isDisposed.Exchange(true))
                return;

            WaitToEnd();

            CurrentScene.Dispose();

            Logger.Info($"Disposed application named: `{Title}`.");

            if (simpleCreated) {
                EntitySchedule.Destroy();

                Graphics.Terminate();
                Logger.Terminate();
            }

            Primitive.Dispose();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return $"{nameof(Application)} {{ {nameof(Title)} = \"{Title}\" }}";
        }

        private void RenderThreadWorker(object? dataObject) {
            CameraData data = (CameraData)dataObject!;

            if (Interlocked.Increment(ref activeRenderers) == 1)
                applicationEndEvent.Reset();

            while (!IsDisposed && !data.IsShouldClose) {
                foreach (EntitySystemBase system in CurrentScene.FrameDependentSystems)
                    system.TryExecute();

                foreach (EntitySystemBase system in CurrentScene.FrameDependentSystems)
                    system.Wait();

                data.MeshRenderer.ExecuteParallelAndWait();
            }

            data.Dispose();

            if (Interlocked.Decrement(ref activeRenderers) == 0)
                applicationEndEvent.Set();
        }

    }
}
