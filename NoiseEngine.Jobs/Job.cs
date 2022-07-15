using System;
using System.Reflection;

namespace NoiseEngine.Jobs {
    public readonly struct Job : IEquatable<Job> {

        private readonly ulong id;

        public Delegate ToExecute { get; }
        public JobTime ExecutionTime { get; }

        internal ulong Id => id;

        /// <summary>
        /// Do not use default constructor for this type, always throws <see cref="InvalidOperationException"/>.
        /// Use JobsWorld.EnqueueJob method instead.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Always throws <see cref="InvalidOperationException"/>.
        /// </exception>
        [Obsolete($"Do not use default constructor for this type. Use {nameof(JobsWorld.EnqueueJob)} method instead.", true)]
        public Job() {
            throw new InvalidOperationException($"Do not use default constructor for this type. Use {nameof(JobsWorld.EnqueueJob)} method instead.");
        }

        internal Job(ulong id, Delegate toExecute, JobTime executionTime) {
            this.id = id;
            ToExecute = toExecute;
            ExecutionTime = executionTime;
        }

        /// <summary>
        /// Destroys this <see cref="Job"/>
        /// </summary>
        /// <param name="world"><see cref="JobsWorld"/> assigned to this <see cref="Job"/></param>
        public void Destroy(JobsWorld world) {
            if (!IsInvoked(world))
                world.queue.DestroyJob(this);
        }

        /// <summary>
        /// Checks if this <see cref="Job"/> was invoked
        /// </summary>
        /// <param name="world"><see cref="JobsWorld"/> assigned to this <see cref="Job"/></param>
        /// <returns>True when this <see cref="Job"/> was invoked or false when not</returns>
        public bool IsInvoked(JobsWorld world) {
            return ExecutionTime.Difference(world.WorldTime) <= 1;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified other <see cref="Job"/>
        /// </summary>
        /// <param name="other">An <see cref="Job"/> to compare to this instance</param>
        /// <returns>True if other <see cref="Job"/> is an instance of <see cref="Job"/> and equals the value of this instance or when not returns false</returns>
        public bool Equals(Job other) {
            return id == other.id;
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation
        /// </summary>
        /// <returns>The string representation of the value of this instance</returns>
        public override string ToString() {
            return $"{nameof(Job)}<{id.ToString("X")}>";
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode() {
            return id.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object
        /// </summary>
        /// <param name="obj">An object to compare to this instance</param>
        /// <returns>True if obj is an instance of <see cref="Job"/> and equals the value of this instance or when not returns false</returns>
        public override bool Equals(object? obj) {
            return obj is Job other && Equals(other);
        }

        internal void Invoke(JobsWorld world) {
            ParameterInfo[] parametersInfo = ToExecute.Method.GetParameters();
            object[] parameters = new object[parametersInfo.Length];

            for (int i = 0; i < parametersInfo.Length; i++) {
                ParameterInfo parameterInfo = parametersInfo[i];
                parameters[i] = world.ComponentsStorage.PopComponent(this, parameterInfo.ParameterType);
            }

            ToExecute.DynamicInvoke(parameters);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Job"/></param>
        /// <param name="right"><see cref="Job"/></param>
        /// <returns>True if left <see cref="Job"/> is an instance of right <see cref="Job"/> and equals the value of this instance or when not returns false</returns>
        public static bool operator ==(Job left, Job right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is not equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Job"/></param>
        /// <param name="right"><see cref="Job"/></param>
        /// <returns>False if left <see cref="Job"/> is an instance of right <see cref="Job"/> and equals the value of this instance or when not returns true</returns>
        public static bool operator !=(Job left, Job right) {
            return !(left == right);
        }

    }
}
