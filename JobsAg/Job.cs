using System;
using System.Reflection;

namespace NoiseStudio.JobsAg {
    public readonly struct Job : IEquatable<Job> {

        private readonly ulong id;

        public Delegate ToExecute { get; }
        public JobTime ExecutionTime { get; }

        internal Job(ulong id, Delegate toExecute) {
            this.id = id;
            ToExecute = toExecute;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified other Entity
        /// </summary>
        /// <param name="other">An Job to compare to this instance</param>
        /// <returns>True if other Job is an instance of Job and equals the value of this instance or when not returns false</returns>
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
        /// <returns>True if obj is an instance of Entity and equals the value of this instance or when not returns false</returns>
        public override bool Equals(object? obj) {
            return obj is Job other && Equals(other);
        }

        internal void Execute(JobWorld world) {
            ParameterInfo[] parametersInfo = ToExecute.Method.GetParameters();
            object[] parameters = new object[parametersInfo.Length];

            for (int i = 0; i < parametersInfo.Length; i++) {
                ParameterInfo parameterInfo = parametersInfo[i];
                parameters[i] = world.ComponentsStorage.PeekComponent(this, parameterInfo.ParameterType);
            }

            ToExecute.DynamicInvoke(parameters);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Job"/></param>
        /// <param name="right"><see cref="Job"/></param>
        /// <returns>True if left Job is an instance of right Job and equals the value of this instance or when not returns false</returns>
        public static bool operator ==(Job left, Job right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is not equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Job"/></param>
        /// <param name="right"><see cref="Job"/></param>
        /// <returns>False if left Job is an instance of right Job and equals the value of this instance or when not returns true</returns>
        public static bool operator !=(Job left, Job right) {
            return !(left == right);
        }

    }
}
