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
using System.Reflection;
using System.Threading;

namespace NoiseEngine {
    public class Application : IDisposable {

        private readonly ConcurrentBag<Window> windows = new ConcurrentBag<Window>();
        private readonly ConcurrentBag<EntitySystemBase> frameDependentSystems = new ConcurrentBag<EntitySystemBase>();
        private readonly ManualResetEvent applicationEndEvent = new ManualResetEvent(false);
        private readonly EntitySchedule schedule;

        private uint activeRenderers;
        private AtomicBool isDisposed;

        private bool simpleCreated;

        public EntityWorld World { get; } = new EntityWorld();
        public string Title { get; }
        public Logger Logger { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public PrimitiveCreator Primitive { get; }

        public bool IsDisposed => isDisposed;
        public IEnumerable<Window> Windows => windows;

        public Application(Logger logger, GraphicsDevice graphicsDevice, EntitySchedule schedule, string title) {
            Logger = logger;
            GraphicsDevice = graphicsDevice;
            Title = title;
            Primitive = new PrimitiveCreator(this);

            this.schedule = schedule;

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
            out Entity cameraEntity, string? title = null, LogType visibleLogs = LogType.AllWithoutTrace
        ) {
            title ??= Assembly.GetEntryAssembly()?.GetName().Name!;

            Logger logger = new Logger(visibleLogs);
            logger.AddHandler(new ConsoleLoggerHandler(new LoggerConfiguration()));
            logger.AddHandler(FileLoggerHandler.CreateLogFileInDirectory("logs"));

            Graphics.Initialize(logger, title, Assembly.GetEntryAssembly()!.GetName().Version!);
            GraphicsDevice graphicsDevice = new GraphicsDevice(false);

            Application application = new Application(logger, graphicsDevice, new EntitySchedule(), title);
            application.simpleCreated = true;

            application.AddFrameDependentSystem(new CameraSystem());

            cameraEntity = application.CreateCamera(new Window(graphicsDevice, new UInt2(1280, 720), title));
            return application;
        }

        /// <summary>
        /// Creates simple default <see cref="Application"/>.
        /// </summary>
        /// <param name="title">Title of <see cref="Application"/>.
        /// By default this will be the name of the entry assembly.</param>
        /// <param name="visibleLogs">Defines what logs will be processed by the handlers.</param>
        /// <returns>New instance of <see cref="Application"/>.</returns>
        public static Application Create(string? title = null, LogType visibleLogs = LogType.AllWithoutTrace) {
            return Create(out _, title, visibleLogs);
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
        public Entity CreateCamera(Window renderTarget) {
            TransformComponent transform = new TransformComponent(new Float3(0, 0, -5));

            Camera camera = new Camera(renderTarget) {
                ProjectionType = ProjectionType.Perspective,
                NearClipPlane = 0.01f,
                FarClipPlane = 1000.0f,
                Position = transform.Position,
                Rotation = transform.Rotation
            };
            windows.Add(renderTarget);

            Thread windowThread = new Thread(RenderThreadWorker) {
                Name = Title
            };
            windowThread.Start((renderTarget, camera));

            return World.NewEntity(transform, new CameraComponent(camera));
        }

        /// <summary>
        /// Initializes and adds <paramref name="system"/> to systems witch will be executed on each render frame.
        /// </summary>
        /// <param name="system"><see cref="EntitySystemBase"/> system witch will be
        /// executed on each render frame.</param>
        public void AddFrameDependentSystem(EntitySystemBase system) {
            system.Initialize(World, schedule);
            frameDependentSystems.Add(system);
        }

        /// <summary>
        /// Disposes this <see cref="Application"/>.
        /// </summary>
        public void Dispose() {
            if (isDisposed.Exchange(true))
                return;

            WaitToEnd();

            World.Dispose();

            Logger.Info($"Disposed application named: `{Title}`.");

            if (simpleCreated) {
                schedule.Destroy();

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

        private void RenderThreadWorker(object? data) {
            (Window renderTarget, Camera camera) = ((Window, Camera))data!;

            Interlocked.Increment(ref activeRenderers);

            MeshRendererSystem meshRenderer = new MeshRendererSystem(GraphicsDevice, camera);
            meshRenderer.Initialize(World, schedule);

            while (!IsDisposed && !renderTarget.GetShouldClose()) {
                foreach (EntitySystemBase system in frameDependentSystems)
                    system.TryExecute();

                foreach (EntitySystemBase system in frameDependentSystems)
                    system.Wait();

                meshRenderer.ExecuteParallelAndWait();
            }

            renderTarget.Destroy();

            if (Interlocked.Decrement(ref activeRenderers) == 0)
                applicationEndEvent.Set();
        }

    }
}
