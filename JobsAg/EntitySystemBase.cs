using System.Collections.Generic;

namespace NoiseStudio.JobsAg {
    public abstract class EntitySystemBase {

        internal List<EntityGroup> groups = new List<EntityGroup>();
        internal double lastExecutionTime = Time.UtcMilliseconds;

        private bool usesSchedule = false;
        private double? cycleTime = 0;
        private EntitySchedule? schedule;

        public double? CycleTime {
            get {
                return cycleTime;
            }
            set {
                cycleTime = value;
                if (cycleTime == null) {
                    if (usesSchedule) {
                        usesSchedule = false;
                        schedule?.RemoveSystem(this);
                    }
                } else {
                    usesSchedule = true;
                    schedule?.AddSystem(this);
                }

                CycleTimeSeconds = (cycleTime ?? 1000.0) / 1000.0;
                CycleTimeSecondsF = (float)CycleTimeSeconds;
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

        protected double DeltaTime { get; private set; } = 1;
        protected float DeltaTimeF { get; private set; } = 1;
        protected double CycleTimeSeconds { get; private set; } = 1;
        protected float CycleTimeSecondsF { get; private set; } = 1;

        /// <summary>
        /// Performs a cycle on this system
        /// </summary>
        public void Execute() {
            InternalExecute();
        }

        internal abstract void InternalUpdateEntity(Entity entity);

        internal virtual void RegisterGroup(EntityGroup group) {
            lock (groups)
                groups.Add(group);
        }

        internal virtual void InternalExecute() {
            InternalUpdate();
        }

        internal virtual void InternalUpdate() {
            double executionTime = Time.UtcMilliseconds;

            DeltaTime = (executionTime - lastExecutionTime) / 1000;
            DeltaTimeF = (float)DeltaTime;

            lastExecutionTime = executionTime;
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
