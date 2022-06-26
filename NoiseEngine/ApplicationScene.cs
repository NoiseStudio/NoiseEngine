using NoiseEngine.Jobs;
using NoiseEngine.Threading;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace NoiseEngine {
    public abstract class ApplicationScene : IDisposable {

        private Application? application;
        private AtomicBool isDisposed;

        public abstract EntityWorld EntityWorld { get; }

        public bool IsDisposed => isDisposed;
        public Application Application => application ?? throw new NullReferenceException(
            $"{nameof(ApplicationScene)} is not initialized. Use {nameof(Initialize)} to initialize scene.");

        internal ConcurrentBag<EntitySystemBase> FrameDependentSystems { get; } = new ConcurrentBag<EntitySystemBase>();

        public void Initialize(Application application) {
            this.application = application;
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

            OnUnload();
            OnTerminate();

            FrameDependentSystems.Clear();
        }

        internal void OnLoadInternal() {
            OnLoad();
        }

        internal void OnUnloadInternal() {
            OnUnload();
        }

        internal bool ReuseWindowInternal(CameraData window, out Entity newCameraEntity) {
            return ReuseWindow(window, out newCameraEntity);
        }

        protected abstract bool ReuseWindow(CameraData window, out Entity newCameraEntity);

        protected virtual void OnLoad() {
        }

        protected virtual void OnUnload() {
        }

        protected virtual void OnTerminate() {
        }

    }
}
