using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseEngine.Mathematics.Helpers;

namespace NoiseEngine.Mathematics;

public static class Quaternion {

    /// <summary>
    /// Converts euler angles in radians to <see cref="Quaternion{T}"/>.
    /// </summary>
    /// <param name="x">Pitch euler angle.</param>
    /// <param name="y">Yaw euler angle.</param>
    /// <param name="z">Roll euler angle.</param>
    /// <returns><see cref="Quaternion{T}"/> representation of angles.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerRadians<T>(T x, T y, T z)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        (T sy, T cy) = T.SinCos(z * NumberHelper<T>.Half);
        (T sp, T cp) = T.SinCos(y * NumberHelper<T>.Half);
        (T sr, T cr) = T.SinCos(x * NumberHelper<T>.Half);

        return new Quaternion<T>(
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy,
            cr * cp * cy + sr * sp * sy
        );
    }

    /// <summary>
    /// Converts euler angles in radians to <see cref="Quaternion{T}"/>.
    /// </summary>
    /// <param name="angles">Euler angle in radians.</param>
    /// <returns><see cref="Quaternion{T}"/> representation of angles.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerRadians<T>(Vector3<T> angles)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        return EulerRadians(angles.X, angles.Y, angles.Z);
    }

    /// <summary>
    /// Converts euler angles in degrees to <see cref="Quaternion{T}"/>.
    /// </summary>
    /// <param name="x">Pitch euler angle.</param>
    /// <param name="y">Yaw euler angle.</param>
    /// <param name="z">Roll euler angle.</param>
    /// <returns><see cref="Quaternion{T}"/> representation of angles.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerDegrees<T>(T x, T y, T z)
        where T : IFloatingPointIeee754<T>
    {
        return EulerRadians(
            FloatingPointIeee754Helper<T>.ConvertDegreesToRadians(x),
            FloatingPointIeee754Helper<T>.ConvertDegreesToRadians(y),
            FloatingPointIeee754Helper<T>.ConvertDegreesToRadians(z)
        );
    }

    /// <summary>
    /// Converts euler angles in degrees to <see cref="Quaternion{T}"/>.
    /// </summary>
    /// <param name="angles">Euler angle in degrees.</param>
    /// <returns><see cref="Quaternion{T}"/> representation of angles.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerDegrees<T>(Vector3<T> angles)
        where T : IFloatingPointIeee754<T>
    {
        return EulerDegrees(angles.X, angles.Y, angles.Z);
    }

    /// <summary>
    /// Creates a rotation which rotates <paramref name="forward"/>s to upwards directions.
    /// Where upwards is always <see cref="Vector3{T}.Up"/>.
    /// </summary>
    /// <param name="forward">The forward direction. Direction to orient towards.</param>
    /// <returns>The calculated <see cref="Quaternion{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> LookRotation<T>(Vector3<T> forward) where T : IFloatingPointIeee754<T> {
        return LookRotation(forward, Vector3<T>.Up);
    }

    /// <summary>
    /// Creates a rotation which rotates <paramref name="forward"/>s to <paramref name="up"/>wards directions.
    /// </summary>
    /// <param name="forward">The forward direction. Direction to orient towards.</param>
    /// <param name="up">
    /// Up direction. Contrains y axis orientation to a plane this vector lies on. This rule might be broken if
    /// <paramref name="forward"/> and <paramref name="up"/> direction are nearly parallel.
    /// </param>
    /// <returns>The calculated <see cref="Quaternion{T}"/>.</returns>
    public static Quaternion<T> LookRotation<T>(Vector3<T> forward, Vector3<T> up) where T : IFloatingPointIeee754<T> {
        forward = forward.Normalize();
        Vector3<T> rightNorm = up.Cross(forward).Normalize();
        Vector3<T> upNorm = forward.Cross(rightNorm);

        T m00 = rightNorm.X;
        T m01 = rightNorm.Y;
        T m02 = rightNorm.Z;
        T m10 = upNorm.X;
        T m11 = upNorm.Y;
        T m12 = upNorm.Z;
        T m20 = forward.X;
        T m21 = forward.Y;
        T m22 = forward.Z;

        T sum = m00 + m11 + m22;
        if (sum > T.Zero) {
            T num = T.Sqrt(sum + T.One);
            T inv = NumberHelper<T>.Half / num;
            return new Quaternion<T>(
                (m12 - m21) * inv,
                (m20 - m02) * inv,
                (m01 - m10) * inv,
                num * NumberHelper<T>.Half
            );
        } else if (m00 >= m11 && m00 >= m22) {
            T num = T.Sqrt(T.One + m00 - m11 - m22);
            T inv = NumberHelper<T>.Half / num;
            return new Quaternion<T>(
                num * NumberHelper<T>.Half,
                (m01 + m10) * inv,
                (m02 + m20) * inv,
                (m12 - m21) * inv
            );
        } else if (m11 > m22) {
            T num = T.Sqrt(T.One + m11 - m00 - m22);
            T inv = NumberHelper<T>.Half / num;
            return new Quaternion<T>(
                (m10 + m01) * inv,
                num * NumberHelper<T>.Half,
                (m21 + m12) * inv,
                (m20 - m02) * inv
            );
        } else {
            T num = T.Sqrt(T.One + m22 - m00 - m11);
            T inv = NumberHelper<T>.Half / num;
            return new Quaternion<T>(
                (m20 + m02) * inv,
                (m21 + m12) * inv,
                num * NumberHelper<T>.Half,
                (m01 - m10) * inv
            );
        }
    }

}
