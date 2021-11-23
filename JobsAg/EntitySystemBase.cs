using System;
using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    public abstract class EntitySystemBase {

        internal int lastExecutionTime = 0;

        private bool usesSchedule = false;
        private uint cycleTime = 0;
        private EntitySchedule? schedule;

        public uint CycleTime {
            get {
                return cycleTime;
            }
            set {
                cycleTime = value;
                if (cycleTime == 0) {
                    if (usesSchedule) {
                        usesSchedule = false;
                        schedule?.RemoveSystem(this);
                    }
                } else {
                    usesSchedule = true;
                    schedule?.AddSystem(this);
                }
            }
        }

        public EntitySchedule? Schedule {
            get {
                return schedule;
            }
            set {
                if (usesSchedule)
                    schedule?.RemoveSystem(this);

                schedule = value;
                if (usesSchedule)
                    schedule?.AddSystem(this);
            }
        }

        public EntityWorld World { get; private set; } = EntityWorld.Empty;

        private protected List<EntityGroup> groups = new List<EntityGroup>();

        /// <summary>
        /// Performs a cycle on this system
        /// </summary>
        public void Execute() {
            InternalExecute();
        }

        internal virtual void RegisterGroup(EntityGroup group) {
            lock (groups)
                groups.Add(group);
        }

        internal virtual void InternalExecute() {
            InternalUpdate();
        }

        internal virtual void InternalUpdate() {
            lastExecutionTime = Environment.TickCount;
            Update();
        }

        internal virtual void InternalInitialize(EntityWorld world, EntitySchedule schedule) {
            World = world;
            Schedule = schedule;

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
