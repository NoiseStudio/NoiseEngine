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

}
