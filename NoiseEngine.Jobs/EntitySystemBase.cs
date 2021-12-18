using System;
using System.Collections.Generic;
using System.Threading;

namespace NoiseEngine.Jobs {
    public abstract class EntitySystemBase {

        internal List<EntityGroup> groups = new List<EntityGroup>();
        internal double lastExecutionTime = Time.UtcMilliseconds;
        internal double cycleTimeWithDelta = 0;
        internal uint cyclesCount = 0;

        private readonly ManualResetEvent workResetEvent = new ManualResetEvent(true);
        private readonly ConcurrentList<EntitySystemBase> dependencies = new ConcurrentList<EntitySystemBase>();
        private readonly Dictionary<EntitySystemBase, uint> dependenciesCyclesCount = new Dictionary<EntitySystemBase, uint>();
        private readonly ConcurrentList<EntitySystemBase> blockadeDependencies = new ConcurrentList<EntitySystemBase>();

        private double? cycleTime = 0;
        private bool usesSchedule = false;
        private EntitySchedule? schedule;
        private bool enabled = true;
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

                OnScheduleChange();
            }
        }

        public bool Enabled {
            get {
                return enabled;
            }
            set {
                if (enabled != value) {
                    enabled = value;
                    if (enabled)
                        InternalStart();
                    else
                        InternalStop();
                }
            }
        }

        public bool CanExecute {
            get {
                if (IsWorking || !Enabled)
                    return false;

                foreach (EntitySystemBase system in dependencies) {
                    if (system.IsWorking || system.cyclesCount == dependenciesCyclesCount[system])
                        return false;
                }
                foreach (EntitySystemBase system in blockadeDependencies) {
                    if (system.IsWorking)
                        return false;
                }

                return true;
            }
        }

        public EntityWorld World { get; private set; } = EntityWorld.Empty;
        public bool IsWorking { get; private set; }

        protected int ThreadId {
            get {
                if (Schedule == null)
                    return 0;
                if (Schedule.threadIds.TryGetValue(Environment.CurrentManagedThreadId, out int threadId))
                    return threadId;
                return 0;
            }
        }

        protected int ThreadCount {
            get {
                if (Schedule == null)
                    return 1;
                return Schedule.threadIdCount;
            }
        }

        protected double DeltaTime { get; private set; } = 1;
        protected float DeltaTimeF { get; private set; } = 1;
        protected double CycleTimeSeconds { get; private set; } = 1;
        protected float CycleTimeSecondsF { get; private set; } = 1;

        /// <summary>
        /// Performs a cycle on this system
        /// </summary>
        public void Execute() {
            AssertCanExecute();
            InternalExecute();
        }

        /// <summary>
        /// Performs a cycle on this system with using schedule threads
        /// </summary>
        public void ExecuteMultithread() {
            AssertCanExecute();
            Wait();
            OrderWork();
            InternalUpdate();
            Schedule!.EnqueuePriorityPackages(this);
            ReleaseWork();
            Wait();
        }

        /// <summary>
        /// Blocks the current thread until the cycle completes
        /// </summary>
        public void Wait() {
            workResetEvent.WaitOne();
        }

        public void AddDependency(EntitySystemBase system) {
            dependenciesCyclesCount.Add(system, uint.MaxValue);
            system.blockadeDependencies.Add(this);
            dependencies.Add(system);
        }

        public void RemoveDependency(EntitySystemBase system) {
            dependencies.Remove(system);
            dependenciesCyclesCount.Remove(system);
            system.blockadeDependencies.Remove(this);
        }

        /// <summary>
        /// Returns a string that represents the current system
        /// </summary>
        /// <returns>A string that represents the current system</returns>
        public override string ToString() {
            return GetType().Name;
        }

        internal abstract void InternalUpdateEntity(Entity entity);

        internal virtual void RegisterGroup(EntityGroup group) {
            lock (groups)
                groups.Add(group);
        }

        internal virtual void InternalExecute() {
            Wait();
            OrderWork();
            InternalUpdate();
        }

        internal virtual void InternalInitialize(EntityWorld world, EntitySchedule schedule) {
            World = world;
            Schedule = schedule;

            OnInitialize();
        }

        internal virtual void InternalStart() {
            OnStart();
        }

        internal virtual void InternalUpdate() {
            foreach (EntitySystemBase system in dependencies) {
                dependenciesCyclesCount[system] = system.cyclesCount;
            }

            cyclesCount++;
            double executionTime = Time.UtcMilliseconds;

            double deltaTimeInMiliseconds = executionTime - lastExecutionTime;
            DeltaTime = deltaTimeInMiliseconds / 1000;
            DeltaTimeF = (float)DeltaTime;

            if (CycleTime != null) {
                cycleTimeWithDelta = (double)CycleTime - (deltaTimeInMiliseconds - (double)CycleTime);
            }

            lastExecutionTime = executionTime;
            OnUpdate();
        }

        internal virtual void InternalLateUpdate() {
            OnLateUpdate();
        }

        internal virtual void InternalStop() {
            OnStop();
        }

        internal virtual void InternalTerminate() {
            OnTerminate();
        }

        internal void OrderWork() {
            if (Interlocked.Increment(ref ongoingWork) == 1) {
                workResetEvent.Reset();
                IsWorking = true;
            }
        }

        internal void ReleaseWork() {
            if (Interlocked.Decrement(ref ongoingWork) == 0) {
                InternalLateUpdate();
                workResetEvent.Set();
                IsWorking = false;
            }
        }

        /// <summary>
        /// This method is executed when this system is creating
        /// </summary>
        protected virtual void OnInitialize() {
        }

        /// <summary>
        /// This method is executed when this system is enabling
        /// </summary>
        protected virtual void OnStart() {
        }

        /// <summary>
        /// This method is executed on begin of every cycle of this system
        /// </summary>
        protected virtual void OnUpdate() {
        }

        /// <summary>
        /// This method is executed on end of every cycle of this system
        /// </summary>
        protected virtual void OnLateUpdate() {
        }

        /// <summary>
        /// This method is executed when this system is disabling
        /// </summary>
        protected virtual void OnStop() {
        }

        /// <summary>
        /// This method is executed when this system is destroying
        /// </summary>
        protected virtual void OnTerminate() {
        }

        /// <summary>
        /// This method is executed when <see cref="EntitySchedule"/> was changed
        /// </summary>
        protected virtual void OnScheduleChange() {
        }

        private void AssertCanExecute() {
            if (!CanExecute)
                throw new InvalidOperationException($"System {ToString()} could not be executed.");
        }

    }
}
