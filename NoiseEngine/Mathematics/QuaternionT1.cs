using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NoiseEngine.Mathematics.Helpers;

namespace NoiseEngine.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Quaternion<T>(T X, T Y, T Z, T W) where T : INumber<T> {

    /// <summary>
    /// Quaternion that represents not rotated object (with euler angles (0, 0, 0)).
    /// </summary>
    public static Quaternion<T> Identity => new Quaternion<T>(T.Zero, T.Zero, T.Zero, T.One);

    /// <summary>
    /// Calculates squared length of this <see cref="Vector2{T}"/>.
    /// </summary>
    /// <returns>Squared length of this <see cref="Vector2{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MagnitudeSquared() {
        return X * X + Y * Y + Z * Z + W * W;
    }

    /// <summary>
    /// Calculates dot product of this <see cref="Quaternion{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Quaternion{T}"/>.</param>
    /// <returns>Dot product of this <see cref="Quaternion{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Quaternion<T> rhs) {
        return X * rhs.X + Y * rhs.Y + Z * rhs.Z + W * rhs.W;
    }

    /// <summary>
    /// Calculates conjugate of this <see cref="Quaternion{T}"/>.
    /// </summary>
    /// <returns>Conjugate of this <see cref="Quaternion{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Quaternion<T> Conjugate() {
        return new Quaternion<T>(-X, -Y, -Z, W);
    }

    /// <summary>
    /// Calculates inverse of this <see cref="Quaternion{T}"/>.
    /// </summary>
    /// <returns>Inverse of this <see cref="Quaternion{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Quaternion<T> Inverse() {
        T inverse = T.One / MagnitudeSquared();
        return Conjugate() * inverse;
    }

    /// <summary>
    /// Adds <paramref name="lhs"/> and <paramref name="rhs"/> together to compute their sum.
    /// </summary>
    /// <param name="lhs">First <see cref="Quaternion{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Quaternion{T}"/>.</param>
    /// <returns>Result of <paramref name="lhs"/> addition with <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> operator +(Quaternion<T> lhs, Quaternion<T> rhs) {
        return new Quaternion<T>(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z, lhs.W + rhs.W);
    }

    /// <summary>
    /// Combines <paramref name="lhs"/> with <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Quaternion{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Quaternion{T}"/>.</param>
    /// <returns><see cref="Quaternion{T}"/> with combined rotation
    /// <paramref name="lhs"/> with <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> operator *(Quaternion<T> lhs, Quaternion<T> rhs) {
        return new Quaternion<T>(
            lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
            lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
            lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
            lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z
        );
    }

    /// <summary>
    /// Rotates <paramref name="point"/> by <paramref name="rotation"/>.
    /// </summary>
    /// <param name="rotation"><see cref="Quaternion{T}"/> that <paramref name="point"/> will be rotated by.</param>
    /// <param name="point">Point that will be rotated.</param>
    /// <returns>Rotated <paramref name="point"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator *(Quaternion<T> rotation, Vector3<T> point) {
        T x = rotation.X * NumberHelper<T>.Value2;
        T y = rotation.Y * NumberHelper<T>.Value2;
        T z = rotation.Z * NumberHelper<T>.Value2;
        T xx = rotation.X * x;
        T yy = rotation.Y * y;
        T zz = rotation.Z * z;
        T xy = rotation.X * y;
        T xz = rotation.X * z;
        T yz = rotation.Y * z;
        T wx = rotation.W * x;
        T wy = rotation.W * y;
        T wz = rotation.W * z;

        return new Vector3<T>(
            (T.One - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z,
            (xy + wz) * point.X + (T.One - (xx + zz)) * point.Y + (yz - wx) * point.Z,
            (xz - wy) * point.X + (yz + wx) * point.Y + (T.One - (xx + yy)) * point.Z
        );
    }

    /// <summary>
    /// Multiplies <paramref name="lhs"/> by <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">Multiplied <see cref="Quaternion{T}"/>.</param>
    /// <param name="rhs">Multiplier of all components.</param>
    /// <returns>Result of <paramref name="lhs"/> multiplication by <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> operator *(Quaternion<T> lhs, T rhs) {
        return new Quaternion<T>(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs, lhs.W * rhs);
    }

}
