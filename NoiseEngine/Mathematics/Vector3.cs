using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Vector3<T>(T X, T Y, T Z) where T : INumber<T> {

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(T.Zero, T.Zero, T.Zero).
    /// </summary>
    public static Vector3<T> Zero => new Vector3<T>(T.Zero, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(T.One, T.One, T.One).
    /// </summary>
    public static Vector3<T> One => new Vector3<T>(T.One, T.One, T.One);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(-T.One, T.Zero, T.Zero).
    /// </summary>
    public static Vector3<T> Left => new Vector3<T>(-T.One, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(T.One, T.Zero, T.Zero).
    /// </summary>
    public static Vector3<T> Right => new Vector3<T>(T.One, T.Zero, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(T.Zero, T.One, T.Zero).
    /// </summary>
    public static Vector3<T> Up => new Vector3<T>(T.Zero, T.One, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(T.Zero, -T.One, T.Zero).
    /// </summary>
    public static Vector3<T> Down => new Vector3<T>(T.Zero, -T.One, T.Zero);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(T.Zero, T.Zero, T.One).
    /// </summary>
    public static Vector3<T> Front => new Vector3<T>(T.Zero, T.Zero, T.One);

    /// <summary>
    /// Shorthand for writing new <see cref="Vector3{T}"/>(T.Zero, T.Zero, -T.One).
    /// </summary>
    public static Vector3<T> Back => new Vector3<T>(T.Zero, T.Zero, -T.One);

    /// <summary>
    /// Calculates squared length of this <see cref="Vector3{T}"/>.
    /// </summary>
    /// <returns>Squared length of this <see cref="Vector3{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MagnitudeSquared() {
        return X * X + Y * Y + Z * Z;
    }

    /// <summary>
    /// Returns greatest component of this <see cref="Vector3{T}"/>.
    /// </summary>
    /// <returns>
    /// <see cref="X"/> if it greater than <see cref="Y"/> or <see cref="Z"/>; otherwise <see cref="Y"/> if it greater
    /// than <see cref="Z"/>; otherwise <see cref="Z"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MaxComponent() {
        return T.Max(X, T.Max(Y, Z));
    }

    /// <summary>
    /// Calculates dot product of this <see cref="Vector3{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector3{T}"/>.</param>
    /// <returns>Dot product of this <see cref="Vector3{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Vector3<T> rhs) {
        return X * rhs.X + Y * rhs.Y + Z * rhs.Z;
    }

    /// <summary>
    /// Calculates squared distance between this <see cref="Vector3{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector3{T}"/>.</param>
    /// <returns>Squared distance between this <see cref="Vector3{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T DistanceSquared(Vector3<T> rhs) {
        return (this - rhs).MagnitudeSquared();
    }

    /// <summary>
    /// Multiplies this <see cref="Vector3{T}"/> by <paramref name="rhs"/> component-wise.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector3{T}"/>.</param>
    /// <returns><see cref="Vector3{T}"/> in which every component of this <see cref="Vector3{T}"/>
    /// is multiplied by the same component of <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> Scale(Vector3<T> rhs) {
        return new Vector3<T>(X * rhs.X, Y * rhs.Y, Z * rhs.Z);
    }

    /// <summary>
    /// Linearly interpolates between this <see cref="Vector3{T}"/> and <paramref name="rhs"/>.
    /// This <see cref="Vector3{T}"/> is start value, returned when <paramref name="t"/> = 0.
    /// </summary>
    /// <param name="rhs">End <see cref="Vector3{T}"/>, returned when <paramref name="t"/> = 1.</param>
    /// <param name="t">Not clamped value used to interpolate between
    /// <see cref="Vector3{T}"/> and <paramref name="rhs"/>.</param>
    /// <returns><see cref="Vector3{T}"/> interpolated value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> Lerp(Vector3<T> rhs, T t) {
        return this + (rhs - this) * t;
    }

    /// <summary>
    /// Calculates cross product of this <see cref="Vector3{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Vector3{T}"/>.</param>
    /// <returns>Cross product of this <see cref="Vector3{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> Cross(Vector3<T> rhs) {
        return new Vector3<T>(
            Y * rhs.Z - Z * rhs.Y,
            Z * rhs.X - X * rhs.Z,
            X * rhs.Y - Y * rhs.X
        );
    }

    /// <summary>
    /// Adds <paramref name="lhs"/> and <paramref name="rhs"/> together to compute their sum.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector3{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector3{T}"/>.</param>
    /// <returns>Result of <paramref name="lhs"/> addition with <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator +(Vector3<T> lhs, Vector3<T> rhs) {
        return new Vector3<T>(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
    }

    /// <summary>
    /// Subtracts <paramref name="lhs"/> from <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector3{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector3{T}"/>.</param>
    /// <returns>Result of <paramref name="lhs"/> subtraction from <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator -(Vector3<T> lhs, Vector3<T> rhs) {
        return new Vector3<T>(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
    }

    /// <summary>
    /// Multiplies <paramref name="lhs"/> by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Multiplied <see cref="Vector3{T}"/>.</param>
    /// <param name="rhs">Multiplier of all components.</param>
    /// <returns>Result of <paramref name="lhs"/> multiplication by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator *(Vector3<T> lhs, T rhs) {
        return new Vector3<T>(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
    }

    /// <summary>
    /// Divides <paramref name="lhs"/> by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Divided <see cref="Vector3{T}"/>.</param>
    /// <param name="rhs">Divider of all components.</param>
    /// <returns>Result of <paramref name="lhs"/> division by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator /(Vector3<T> lhs, T rhs) {
        return new Vector3<T>(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
    }

    /// <summary>
    /// Returns the remainder of <paramref name="lhs"/> division by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Divided <see cref="Vector3{T}"/>.</param>
    /// <param name="rhs">Divider of all components.</param>
    /// <returns>The remainder of <paramref name="lhs"/> divided-by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator %(Vector3<T> lhs, T rhs) {
        return new Vector3<T>(lhs.X % rhs, lhs.Y % rhs, lhs.Z % rhs);
    }

}
