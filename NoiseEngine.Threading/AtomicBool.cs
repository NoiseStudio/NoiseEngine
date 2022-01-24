using System;
using System.Threading;

namespace NoiseEngine.Threading {
    public struct AtomicBool : IEquatable<AtomicBool> {

        private int value;

        public bool Value => value != 0;

        public AtomicBool(bool initialValue) {
            value = initialValue ? 1 : 0;
        }

        /// <summary>
        /// Sets this <see cref="AtomicBool"/> to a specified value and returns the original value,
        /// as an atomic operation.
        /// </summary>
        /// <param name="value">The value to which this <see cref="AtomicBool"/> is set.</param>
        /// <returns>The original value of this <see cref="AtomicBool"/>.</returns>
        public bool Exchange(bool value) {
            return Interlocked.Exchange(ref this.value, value ? 1 : 0) != 0;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified other <see cref="AtomicBool"/>.
        /// </summary>
        /// <param name="other">An <see cref="AtomicBool"/> to compare to this instance.</param>
        /// <returns>
        /// <see langword="true"/> if obj is an instance of <see cref="AtomicBool"/>and equals
        /// the value of this instance, otherwise <see langword="false"/>.
        /// </returns>
        public bool Equals(AtomicBool other) {
            return value == other.value;
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified object.
        /// </summary>
        /// <param name="obj">An object to compare to this instance.</param>
        /// <returns>
        /// <see langword="true"/> if obj is an instance of <see cref="AtomicBool"/>and equals
        /// the value of this instance, otherwise <see langword="false"/>.
        /// </returns>
        public override bool Equals(object? obj) {
            return obj is AtomicBool other && Equals(other);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() {
            return value.GetHashCode();
        }

        /// <summary>
        /// Converts this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation of this instance.</returns>
        public override string ToString() {
            return Value.ToString();
        }

        /// <summary>
        /// Returns a value indicating whether <paramref name="left"/> is equal to a <paramref name="right"/>.
        /// </summary>
        /// <param name="left">First <see cref="AtomicBool"/>.</param>
        /// <param name="right">Second <see cref="AtomicBool"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> is equal to <paramref name="right"/>,
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator ==(AtomicBool left, AtomicBool right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Returns a value indicating whether <paramref name="left"/> is not equal to a <paramref name="right"/>.
        /// </summary>
        /// <param name="left">First <see cref="AtomicBool"/>.</param>
        /// <param name="right">Second <see cref="AtomicBool"/>.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="left"/> is not equal to <paramref name="right"/>,
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool operator !=(AtomicBool left, AtomicBool right) {
            return !left.Equals(right);
        }

        /// <summary>
        /// Converts <paramref name="atomicBool"/> to <see cref="bool"/>.
        /// </summary>
        /// <param name="atomicBool"><see cref="AtomicBool"/> to convert.</param>
        public static implicit operator bool(AtomicBool atomicBool) {
            return atomicBool.Value;
        }

        /// <summary>
        /// Converts <paramref name="b"/> to <see cref="AtomicBool"/>.
        /// </summary>
        /// <param name="b"><see cref="bool"/> to convert.</param>
        public static implicit operator AtomicBool(bool b) {
            return new AtomicBool(b);
        }

    }
}
