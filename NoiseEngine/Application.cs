using NoiseEngine.Collections.Concurrent;
using NoiseEngine.Jobs;
using NoiseEngine.Logging;
using NoiseEngine.Logging.Standard;
using NoiseEngine.Primitives;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Presentation;
using NoiseEngine.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace NoiseEngine {
    public class Application : IDisposable {

        internal RenderCamera? mainWindow;

        private readonly ConcurrentHashSet<ApplicationScene> loadedScenes = new ConcurrentHashSet<ApplicationScene>();
        private readonly ManualResetEvent applicationEndEvent = new ManualResetEvent(true);

        private uint activeRenderers;
        private AtomicBool isDisposed;
        private bool simpleCreated;

        public EntitySchedule EntitySchedule { get; }
        public string ApplicationName { get; }
        public Logger Logger { get; }
        public GraphicsDevice GraphicsDevice { get; }

        public RenderCamera? MainWindow {
            get => mainWindow;
            set => mainWindow = value ?? throw new ArgumentNullException();
        }

        public bool IsDisposed => isDisposed;
        public IEnumerable<ApplicationScene> LoadedScenes => loadedScenes;
        public IEnumerable<Window> Windows => LoadedScenes.SelectMany(x => x.Cameras).Select(x => x.RenderTarget);

        internal PrimitiveCreatorShared PrimitiveShared { get; }

        public Application(
            Logger logger, GraphicsDevice graphicsDevice,
            EntitySchedule entitySchedule, string applicationName
        ) {
            Logger = logger;
            GraphicsDevice = graphicsDevice;
            ApplicationName = applicationName;
            PrimitiveShared = new PrimitiveCreatorShared(this);
            EntitySchedule = entitySchedule;

            Logger.Info($"Created application named: `{ApplicationName}`.");
        }

        /// <summary>
        /// Creates simple default <see cref="Application"/>.
        /// </summary>
        /// <param name="cameraEntity"><see cref="Entity"/> with <see cref="Camera"/>.</param>
        /// <param name="title">Title of <see cref="Application"/>.
        /// By default this will be the name of the entry assembly.</param>
        /// <param name="visibleLogs">Defines what logs will be processed by the handlers.</param>
        /// <returns>New instance of <see cref="Application"/>.</returns>
        public static Application Create(string? applicationName = null, LogType visibleLogs = LogType.AllWithoutTrace) {
            applicationName ??= Assembly.GetEntryAssembly()?.GetName().Name!;

            Logger logger = new Logger(visibleLogs);
            logger.AddHandler(new ConsoleLoggerHandler(new LoggerConfiguration()));
            logger.AddHandler(FileLoggerHandler.CreateLogFileInDirectory("logs"));

            Graphics.Initialize(logger, applicationName, Assembly.GetEntryAssembly()!.GetName().Version!);
            GraphicsDevice graphicsDevice = new GraphicsDevice(false);

            Application application = new Application(logger, graphicsDevice, new EntitySchedule(), applicationName);
            application.simpleCreated = true;

            return application;
        }

        /// <summary>
        /// Blocks the current thread until the <see cref="Application"/> ends work.
        /// </summary>
        public void WaitToEnd() {
            applicationEndEvent.WaitOne();
        }

        /// <summary>
        /// Disposes this <see cref="Application"/>.
        /// </summary>
        public void Dispose() {
            if (isDisposed.Exchange(true))
                return;

            WaitToEnd();

            foreach (ApplicationScene scene in LoadedScenes)
                scene.Dispose();

            PrimitiveShared.Dispose();
            Logger.Info($"Disposed application named: `{ApplicationName}`.");

            if (simpleCreated) {
                EntitySchedule.Destroy();

                Graphics.Terminate();
                Logger.Terminate();
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            return $"{nameof(Application)} {{ {nameof(ApplicationName)} = \"{ApplicationName}\" }}";
        }

        internal void IncrementActiveRenderers() {
            if (Interlocked.Increment(ref activeRenderers) == 1)
                applicationEndEvent.Reset();
        }

        internal void DecrementActiveRenderers() {
            if (Interlocked.Decrement(ref activeRenderers) == 0)
                applicationEndEvent.Set();
        }

        internal void AddSceneToLoaded(ApplicationScene scene) {
            loadedScenes.Add(scene);
        }

        internal void RemoveSceneFromLoaded(ApplicationScene scene) {
            loadedScenes.Remove(scene);
        }

    }
}
