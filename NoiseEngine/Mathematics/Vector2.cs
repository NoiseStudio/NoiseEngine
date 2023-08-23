using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector2<T>(T X, T Y) where T : INumber<T> {

    /// <summary>
    /// Shorthand for writing new <see cref="Vector2{T}"/>(T.Zero, T.Zero).
    /// </summary>
    public static Vector2<T> Zero => new Vector2<T>(T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector2{T}"/>(T.One, T.One).
    /// </summary>
    public static Vector2<T> One => new Vector2<T>(T.One, T.One);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector2{T}"/>(-T.One, T.Zero).
    /// </summary>
    public static Vector2<T> Left => new Vector2<T>(-T.One, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector2{T}"/>(T.One, T.Zero).
    /// </summary>
    public static Vector2<T> Right => new Vector2<T>(T.One, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector2{T}"/>(T.Zero, T.One).
    /// </summary>
    public static Vector2<T> Up => new Vector2<T>(T.Zero, T.One);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector2{T}"/>(T.Zero, -T.One).
    /// </summary>
    public static Vector2<T> Down => new Vector2<T>(T.Zero, -T.One);

    /// <summary>
    /// Calculates squared length of this <see cref="Vector2{T}"/>.
    /// </summary>
    /// <returns>Squared length of this <see cref="Vector2{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MagnitudeSquared() {
        return X * X + Y * Y;
    }

    /// <summary>
    /// Calculates dot product of this <see cref="Vector2{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector2{T}"/>.</param>
    /// <returns>Dot product of this <see cref="Vector2{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Vector2<T> rhs) {
        return X * rhs.X + Y * rhs.Y;
    }

    /// <summary>
    /// Calculates squared distance between this <see cref="Vector2{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector2{T}"/>.</param>
    /// <returns>Squared distance between this <see cref="Vector2{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T DistanceSquared(Vector2<T> rhs) {
        return (this - rhs).MagnitudeSquared();
    }

    /// <summary>
    /// Multiplies this <see cref="Vector2{T}"/> by <paramref name="rhs"/> component-wise.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector2{T}"/>.</param>
    /// <returns><see cref="Vector2{T}"/> in which every component of this <see cref="Vector2{T}"/>
    /// is multiplied by the same component of <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2<T> Scale(Vector2<T> rhs) {
        return new Vector2<T>(X * rhs.X, Y * rhs.Y);
    }

    /// <summary>
    /// Linearly interpolates between this <see cref="Vector2{T}"/> and <paramref name="rhs"/>.
    /// This <see cref="Vector2{T}"/> is start value, returned when <paramref name="t"/> = 0.
    /// </summary>
    /// <param name="rhs">End <see cref="Vector2{T}"/>, returned when <paramref name="t"/> = 1.</param>
    /// <param name="t">Not clamped value used to interpolate between
    /// <see cref="Vector2{T}"/> and <paramref name="rhs"/>.</param>
    /// <returns><see cref="Vector2{T}"/> interpolated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2<T> Lerp(Vector2<T> rhs, T t) {
        return this + (rhs - this) * t;
    }

    /// <summary>
    /// Adds <paramref name="lhs"/> and <paramref name="rhs"/> together to compute their sum.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector2{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector2{T}"/>.</param>
    /// <returns>Result of <paramref name="lhs"/> addition with <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> operator +(Vector2<T> lhs, Vector2<T> rhs) {
        return new Vector2<T>(lhs.X + rhs.X, lhs.Y + rhs.Y);
    }

    /// <summary>
    /// Subtracts <paramref name="lhs"/> from <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector2{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector2{T}"/>.</param>
    /// <returns>Result of <paramref name="lhs"/> subtraction from <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> operator -(Vector2<T> lhs, Vector2<T> rhs) {
        return new Vector2<T>(lhs.X - rhs.X, lhs.Y - rhs.Y);
    }

    /// <summary>
    /// Multiplies <paramref name="lhs"/> by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Multiplied <see cref="Vector2{T}"/>.</param>
    /// <param name="rhs">Multiplier of all components.</param>
    /// <returns>Result of <paramref name="lhs"/> multiplication by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> operator *(Vector2<T> lhs, T rhs) {
        return new Vector2<T>(lhs.X * rhs, lhs.Y * rhs);
    }

    /// <summary>
    /// Divides <paramref name="lhs"/> by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Divided <see cref="Vector2{T}"/>.</param>
    /// <param name="rhs">Divider of all components.</param>
    /// <returns>Result of <paramref name="lhs"/> division by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> operator /(Vector2<T> lhs, T rhs) {
        return new Vector2<T>(lhs.X / rhs, lhs.Y / rhs);
    }

    /// <summary>
    /// Returns the remainder of <paramref name="lhs"/> division by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Divided <see cref="Vector2{T}"/>.</param>
    /// <param name="rhs">Divider of all components.</param>
    /// <returns>The remainder of <paramref name="lhs"/> divided-by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> operator %(Vector2<T> lhs, T rhs) {
        return new Vector2<T>(lhs.X % rhs, lhs.Y % rhs);
    }

    /// <summary>
    /// Computes the unary negation of a value.
    /// </summary>
    /// <param name="value">The value for which to compute its unary negation.</param>
    /// <returns>The unary negation of <paramref name="value" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> operator -(Vector2<T> value) {
        return new Vector2<T>(-value.X, -value.Y);
    }

    /// <summary>
    /// Computes the unary plus of a value.
    /// </summary>
    /// <param name="value">The value for which to compute the unary plus.</param>
    /// <returns>The unary plus of value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> operator +(Vector2<T> value) {
        return new Vector2<T>(+value.X, +value.Y);
    }

}
