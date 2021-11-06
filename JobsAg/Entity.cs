using System;

namespace NoiseStudio.JobsAg {
    public readonly struct Entity : IEquatable<Entity> {

        private readonly ulong id;

        internal Entity(ulong id) {
            this.id = id;
        }

        /// <summary>
        /// Converts the numeric value of this instance to its equivalent string representation
        /// </summary>
        /// <returns>The string representation of the value of this instance</returns>
        public override string ToString() {
            return $"{nameof(Entity)}<{id.ToString("X")}>";
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
            return obj is Entity other && Equals(other);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified other Entity
        /// </summary>
        /// <param name="other">An Entity to compare to this instance</param>
        /// <returns>True if other Entity is an instance of Entity and equals the value of this instance or when not returns false</returns>
        public bool Equals(Entity other) {
            return id == other.id;
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Entity"/></param>
        /// <param name="right"><see cref="Entity"/></param>
        /// <returns>True if left Entity is an instance of right Entity and equals the value of this instance or when not returns false</returns>
        public static bool operator ==(Entity left, Entity right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating whether this instance left is not equal to a instance right
        /// </summary>
        /// <param name="left"><see cref="Entity"/></param>
        /// <param name="right"><see cref="Entity"/></param>
        /// <returns>False if left Entity is an instance of right Entity and equals the value of this instance or when not returns true</returns>
        public static bool operator !=(Entity left, Entity right) {
            return !(left == right);
        }

    }
}
