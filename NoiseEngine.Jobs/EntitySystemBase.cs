using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NoiseEngine.Threading;

namespace NoiseEngine.Jobs {
    public abstract class EntitySystemBase {

        internal EntityQueryBase? query;
        internal double lastExecutionTime = Time.UtcMilliseconds;
        internal double cycleTimeWithDelta = 0;
        internal uint cyclesCount = 0;

        private readonly ManualResetEvent workResetEvent = new ManualResetEvent(true);
        private readonly ConcurrentList<EntitySystemBase> dependencies = new ConcurrentList<EntitySystemBase>();
        private readonly Dictionary<EntitySystemBase, uint> dependenciesCyclesCount = new Dictionary<EntitySystemBase, uint>();
        private readonly ConcurrentList<EntitySystemBase> blockadeDependencies = new ConcurrentList<EntitySystemBase>();

        private EntitySchedule? schedule;
        private IEntityFilter? filter;
        private double? cycleTime = 0;
        private bool usesSchedule = false;
        private bool enabled = true;
        private int ongoingWork = 0;
        private AtomicBool isWorking;

        public double? CycleTime {
            get => cycleTime;
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
            get => schedule;
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
            get => enabled;
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

        public IEntityFilter? Filter {
            get => filter;
            set {
                if (query != null)
                    query.Filter = value;
                filter = value;
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
        public bool IsWorking => isWorking;

        protected int ThreadId {
            get {
                EntitySchedule? schedule = Schedule;
                if (schedule == null)
                    return 0;
                if (schedule.threadIds.TryGetValue(Environment.CurrentManagedThreadId, out int threadId))
                    return threadId;
                return 0;
            }
        }

        protected int ThreadCount {
            get {
                EntitySchedule? schedule = Schedule;
                if (schedule == null)
                    return 1;
                return schedule.threadIdCount;
            }
        }

        protected double DeltaTime { get; private set; } = 1;
        protected float DeltaTimeF { get; private set; } = 1;
        protected double CycleTimeSeconds { get; private set; } = 1;
        protected float CycleTimeSecondsF { get; private set; } = 1;

        /// <summary>
        /// Tries to performs a cycle on this system and waits for it to finish.
        /// </summary>
        public void ExecuteAndWait() {
            if (!TryExecuteAndWait())
                ThrowCouldNotExecuteException();
        }

        /// <summary>
        /// Tries to performs a cycle on this system with using schedule threads and waits for it to finish.
        /// </summary>
        public void ExecuteParallelAndWait() {
            if (!TryExecuteParallelAndWait())
                ThrowCouldNotExecuteException();
        }

        /// <summary>
        /// Tries to performs a cycle on this system with using schedule threads.
        /// </summary>
        public void Execute() {
            if (!TryExecute())
                ThrowCouldNotExecuteException();
        }

        /// <summary>
        /// Tries to performs a cycle on this system and waits for it to finish.
        /// </summary>
        /// <returns><see langword="true"/> if system was executed; otherwise false.</returns>
        public bool TryExecuteAndWait() {
            if (!CheckIfCanExecuteAndOrderWork())
                return false;

            InternalExecute();
            ReleaseWork();

            return true;
        }

        /// <summary>
        /// Tries to performs a cycle on this system with using schedule threads and waits for it to finish.
        /// </summary>
        /// <returns><see langword="true"/> if system was executed; otherwise false.</returns>
        public bool TryExecuteParallelAndWait() {
            EntitySchedule schedule = GetEntityScheduleOrThrowException();

            if (!CheckIfCanExecuteAndOrderWork())
                return false;

            InternalUpdate();
            schedule.EnqueuePackages(this);
            ReleaseWork();

            Wait();
            return true;
        }

        /// <summary>
        /// Tries to performs a cycle on this system with using schedule threads.
        /// </summary>
        /// <returns><see langword="true"/> if work was queued; otherwise false.</returns>
        public bool TryExecute() {
            EntitySchedule schedule = GetEntityScheduleOrThrowException();

            if (!CheckIfCanExecuteAndOrderWork())
                return false;

            Task.Run(() => {
                InternalUpdate();
                schedule.EnqueuePackages(this);
                ReleaseWork();
            });

            return true;
        }

        /// <summary>
        /// Blocks the current thread until the cycle completes
        /// </summary>
        public void Wait() {
            workResetEvent.WaitOne();
        }

        /// <summary>
        /// Adds a dependency of this system on <paramref name="system"/> from argument.
        /// This affects the execution of this system as dependencies must be executed first.
        /// </summary>
        /// <param name="system"><see cref="EntitySystemBase"/> add for dependencies.</param>
        public void AddDependency(EntitySystemBase system) {
            dependenciesCyclesCount.Add(system, uint.MaxValue);
            system.blockadeDependencies.Add(this);
            dependencies.Add(system);
        }

        /// <summary>
        /// Removes a dependency of this system on <paramref name="system"/> from argument.
        /// </summary>
        /// <param name="system"><see cref="EntitySystemBase"/> remove for dependencies.</param>
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

        internal virtual void InternalExecute() {
            OrderWork();
            InternalUpdate();
        }

        internal virtual void InternalInitialize(EntityWorld world, EntitySchedule? schedule) {
            World = world;
            if (schedule != null)
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
                isWorking = true;
            }
        }

        internal void ReleaseWork() {
            if (Interlocked.Decrement(ref ongoingWork) == 0) {
                InternalLateUpdate();
                workResetEvent.Set();
                isWorking = false;
            }
        }

        internal bool CheckIfCanExecuteAndOrderWork() {
            if (!Enabled || isWorking.Exchange(true))
                return false;

            foreach (EntitySystemBase system in dependencies) {
                if (system.IsWorking || system.cyclesCount == dependenciesCyclesCount[system]) {
                    isWorking.Exchange(false);
                    return false;
                }
            }
            foreach (EntitySystemBase system in blockadeDependencies) {
                if (system.IsWorking) {
                    isWorking.Exchange(false);
                    return false;
                }
            }

            OrderWork();
            return true;
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

        private void ThrowCouldNotExecuteException() {
            throw new InvalidOperationException($"System {ToString()} could not be executed.");
        }

        private EntitySchedule GetEntityScheduleOrThrowException() {
            EntitySchedule? schedule = Schedule;
            if (schedule == null)
                throw new InvalidOperationException($"{nameof(EntitySchedule)} assigned to this {ToString()} is null.");
            return schedule;
        }

    }
}
