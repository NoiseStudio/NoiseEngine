using System.Collections.Generic;
using System.Threading;

namespace NoiseStudio.JobsAg {
    public abstract class EntitySystemBase {

        internal List<EntityGroup> groups = new List<EntityGroup>();
        internal double lastExecutionTime = Time.UtcMilliseconds;
        internal double cycleTimeWithDelta = 0;

        private readonly ManualResetEvent manualResetEvent = new ManualResetEvent(true);

        private bool usesSchedule = false;
        private double? cycleTime = 0;
        private EntitySchedule? schedule;
        private int ongoingWork = 0;

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
                    cycleTimeWithDelta = (double)cycleTime;
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
        public bool IsWorking { get; private set; }

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

        /// <summary>
        /// Blocks the current thread until the cycle completes
        /// </summary>
        public void Wait() {
            manualResetEvent.WaitOne();
        }

        internal abstract void InternalUpdateEntity(Entity entity);

        internal virtual void RegisterGroup(EntityGroup group) {
            lock (groups)
                groups.Add(group);
        }

        internal virtual void InternalExecute() {
            InternalUpdate();
        }

        internal virtual void InternalInitialize(EntityWorld world, EntitySchedule schedule) {
            World = world;
            Schedule = schedule;

            Initialize();
        }

        internal virtual void InternalStart() {
            Start();
        }

        internal virtual void InternalUpdate() {
            double executionTime = Time.UtcMilliseconds;

            double deltaTimeInMiliseconds = executionTime - lastExecutionTime;
            DeltaTime = deltaTimeInMiliseconds / 1000;
            DeltaTimeF = (float)DeltaTime;

            if (CycleTime != null) {
                cycleTimeWithDelta = (double)CycleTime - (deltaTimeInMiliseconds - (double)CycleTime);
            }

            lastExecutionTime = executionTime;
            Update();
        }

        internal virtual void InternalLateUpdate() {
            LateUpdate();
        }

        internal virtual void InternalStop() {
            Stop();
        }

        internal virtual void InternalTerminate() {
            Terminate();
        }

        internal void OrderWork() {
            if (Interlocked.Increment(ref ongoingWork) == 1) {
                manualResetEvent.Reset();
                IsWorking = true;
            }
        }

        internal void ReleaseWork() {
            if (Interlocked.Decrement(ref ongoingWork) == 0) {
                InternalLateUpdate();
                manualResetEvent.Set();
                IsWorking = false;
            }
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
        /// This method is executed on begin of every cycle of this system
        /// </summary>
        protected virtual void Update() {
        }

        /// <summary>
        /// This method is executed on end of every cycle of this system
        /// </summary>
        protected virtual void LateUpdate() {
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
