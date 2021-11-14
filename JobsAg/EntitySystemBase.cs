using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    public abstract class EntitySystemBase {

        public EntityWorld World { get; private set; } = EntityWorld.Empty;

        private protected List<EntityGroup> groups = new List<EntityGroup>();

        internal virtual void RegisterGroup(EntityGroup group) {
            lock (groups)
                groups.Add(group);
        }

        internal virtual void Execute() {
            Update();
        }

        internal virtual void InternalInitialize(EntityWorld world) {
            World = world;
            Initialize();
        }

        internal virtual void InternalStart() {
            Start();
        }

        internal virtual void InternalStop() {
            Stop();
        }

        internal virtual void InternalTerminate() {
            Terminate();
        }

        /// <summary>
        /// This method is executed when this system is creating
        /// </summary>
        protected virtual void Initialize() {
        }

        /// <summary>
        /// This method is executed when this system is enabling
        /// </summary>
        protected virtual void Start() {
        }

        /// <summary>
        /// This method is executed every cycle of this system
        /// </summary>
        protected virtual void Update() {
        }

        /// <summary>
        /// This method is executed when this system is disabling
        /// </summary>
        protected virtual void Stop() {
        }

        /// <summary>
        /// This method is executed when this system is destroying
        /// </summary>
        protected virtual void Terminate() {
        }

    }
}
