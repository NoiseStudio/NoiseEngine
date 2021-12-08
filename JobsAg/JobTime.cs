using System;

namespace NoiseStudio.JobsAg {
    public struct JobTime : IEquatable<JobTime> {

        internal ulong Time { get; }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified other <see cref="JobTime"/>
        /// </summary>
        /// <param name="other">An <see cref="JobTime"/> to compare to this instance</param>
        /// <returns>True if other <see cref="JobTime"/> is an instance of <see cref="JobTime"/> and equals the value of this instance or when not returns false</returns>
        public bool Equals(JobTime other) {
            return Time == other.Time;
        }

        /// <summary>
        /// Returns the hash code for this instance
        /// </summary>
        /// <returns>A 32-bit signed integer hash code</returns>
        public override int GetHashCode() {
            return Time.GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object
        /// </summary>
        /// <param name="obj">An object to compare to this instance</param>
        /// <returns>True if obj is an instance of <see cref="JobTime"/> and equals the value of this instance or when not returns false</returns>
        public override bool Equals(object? obj) {
            return obj is JobTime other && Equals(other);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="JobTime"/></param>
        /// <param name="right"><see cref="JobTime"/></param>
        /// <returns>True if left <see cref="JobTime"/> is an instance of right <see cref="JobTime"/> and equals the value of this instance or when not returns false</returns>
        public static bool operator ==(JobTime left, JobTime right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is not equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="JobTime"/></param>
        /// <param name="right"><see cref="JobTime"/></param>
        /// <returns>False if left <see cref="JobTime"/> is an instance of right <see cref="JobTime"/> and equals the value of this instance or when not returns true</returns>
        public static bool operator !=(JobTime left, JobTime right) {
            return !(left == right);
        }

        /// <summary>
        /// Returns a value indicating whether the left struct is later than the right struct
        /// </summary>
        /// <param name="left"><see cref="JobTime"/></param>
        /// <param name="right"><see cref="JobTime"/></param>
        /// <returns>True if left <see cref="JobTime"/> is later than right <see cref="JobTime"/> or when not returns false</returns>
        public static bool operator >(JobTime left, JobTime right) {
            return left.Time > right.Time;
        }

        /// <summary>
        /// Returns a value indicating whether the left struct is earlier than the right struct
        /// </summary>
        /// <param name="left"><see cref="JobTime"/></param>
        /// <param name="right"><see cref="JobTime"/></param>
        /// <returns>True if left <see cref="JobTime"/> is earlier than right <see cref="JobTime"/> or when not returns false</returns>
        public static bool operator <(JobTime left, JobTime right) {
            return left.Time < right.Time;
        }

    }
}
