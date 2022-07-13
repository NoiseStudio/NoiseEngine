using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public readonly record struct Vector4<T>(T X, T Y, T Z, T W) where T : INumber<T> {

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.Zero, T.Zero, T.Zero, T.Zero).
    /// </summary>
    public static Vector4<T> Zero => new Vector4<T>(T.Zero, T.Zero, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.One, T.One, T.One, T.One).
    /// </summary>
    public static Vector4<T> One => new Vector4<T>(T.One, T.One, T.One, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(-T.One, T.Zero, T.Zero, T.Zero).
    /// </summary>
    public static Vector4<T> Left => new Vector4<T>(-T.One, T.Zero, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.One, T.Zero, T.Zero, T.Zero).
    /// </summary>
    public static Vector4<T> Right => new Vector4<T>(T.One, T.Zero, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.Zero, T.One, T.Zero, T.Zero).
    /// </summary>
    public static Vector4<T> Up => new Vector4<T>(T.Zero, T.One, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.Zero, -T.One, T.Zero, T.Zero).
    /// </summary>
    public static Vector4<T> Down => new Vector4<T>(T.Zero, -T.One, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.Zero, T.Zero, T.One, T.Zero).
    /// </summary>
    public static Vector4<T> Front => new Vector4<T>(T.Zero, T.Zero, T.One, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.Zero, T.Zero, -T.One, T.Zero).
    /// </summary>
    public static Vector4<T> Back => new Vector4<T>(T.Zero, T.Zero, -T.One, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.Zero, T.Zero, T.Zero, T.One).
    /// </summary>
    public static Vector4<T> Ana => new Vector4<T>(T.Zero, T.Zero, T.Zero, T.One);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector4{T}"/>(T.Zero, T.Zero, T.Zero, -T.One).
    /// </summary>
    public static Vector4<T> Kata => new Vector4<T>(T.Zero, T.Zero, T.Zero, -T.One);

    /// <summary>
    /// Calculates squared length of this <see cref="Vector4{T}"/>.
    /// </summary>
    /// <returns>Squared length of this <see cref="Vector4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MagnitudeSquared() {
        return X * X + Y * Y + Z * Z + W * W;
    }

    /// <summary>
    /// Calculates dot product of this <see cref="Vector4{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector4{T}"/>.</param>
    /// <returns>Dot product of this <see cref="Vector4{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Vector4<T> rhs) {
        return X * rhs.X + Y * rhs.Y + Z * rhs.Z + W * rhs.W;
    }

    /// <summary>
    /// Calculates squared distance between this <see cref="Vector4{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector4{T}"/>.</param>
    /// <returns>Squared distance between this <see cref="Vector4{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T DistanceSquared(Vector4<T> rhs) {
        return (this - rhs).MagnitudeSquared();
    }

    /// <summary>
    /// Multiplies this <see cref="Vector4{T}"/> by <paramref name="rhs"/> component-wise.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector4{T}"/>.</param>
    /// <returns><see cref="Vector4{T}"/> in which every component of this <see cref="Vector4{T}"/>
    /// is multiplied by the same component of <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4<T> Scale(Vector4<T> rhs) {
        return new Vector4<T>(X * rhs.X, Y * rhs.Y, Z * rhs.Z, W * rhs.W);
    }

    /// <summary>
    /// Linearly interpolates between this <see cref="Vector4{T}"/> and <paramref name="rhs"/>.
    /// This <see cref="Vector4{T}"/> is start value, returned when <paramref name="t"/> = 0.
    /// </summary>
    /// <param name="rhs">End <see cref="Vector4{T}"/>, returned when <paramref name="t"/> = 1.</param>
    /// <param name="t">Not clamped value used to interpolate between
    /// <see cref="Vector4{T}"/> and <paramref name="rhs"/>.</param>
    /// <returns><see cref="Vector4{T}"/> interpolated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector4<T> Lerp(Vector4<T> rhs, T t) {
        return this + (rhs - this) * t;
    }

    /// <summary>
    /// Adds <paramref name="lhs"/> and <paramref name="rhs"/> together to compute their sum.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector4{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector4{T}"/>.</param>
    /// <returns>Result of <paramref name="lhs"/> addition with <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4<T> operator +(Vector4<T> lhs, Vector4<T> rhs) {
        return new Vector4<T>(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
    }

    /// <summary>
    /// Subtracts <paramref name="lhs"/> from <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector4{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector4{T}"/>.</param>
    /// <returns>Result of <paramref name="lhs"/> subtraction from <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4<T> operator -(Vector4<T> lhs, Vector4<T> rhs) {
        return new Vector4<T>(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z, lhs.W - rhs.W);
    }

    /// <summary>
    /// Multiplies <paramref name="lhs"/> by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Multiplied <see cref="Vector4{T}"/>.</param>
    /// <param name="rhs">Multiplier of all components.</param>
    /// <returns>Result of <paramref name="lhs"/> multiplication by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4<T> operator *(Vector4<T> lhs, T rhs) {
        return new Vector4<T>(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
    }

    /// <summary>
    /// Divides <paramref name="lhs"/> by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Divided <see cref="Vector4{T}"/>.</param>
    /// <param name="rhs">Divider of all components.</param>
    /// <returns>Result of <paramref name="lhs"/> division by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4<T> operator /(Vector4<T> lhs, T rhs) {
        return new Vector4<T>(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs, lhs.W / rhs);
    }

    /// <summary>
    /// Returns the remainder of <paramref name="lhs"/> division by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Divided <see cref="Vector4{T}"/>.</param>
    /// <param name="rhs">Divider of all components.</param>
    /// <returns>The remainder of <paramref name="lhs"/> divided-by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4<T> operator %(Vector4<T> lhs, T rhs) {
        return new Vector4<T>(lhs.X % rhs, lhs.Y % rhs, lhs.Z % rhs, lhs.W % rhs);
    }

}
