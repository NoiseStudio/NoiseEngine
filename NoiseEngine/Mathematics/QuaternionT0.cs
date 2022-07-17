using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class Quaternion {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerRadians<T>(T x, T y, T z)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        T cy = T.Cos(z * NumberHelper<T>.Half);
        T sy = T.Sin(z * NumberHelper<T>.Half);
        T cp = T.Cos(y * NumberHelper<T>.Half);
        T sp = T.Sin(y * NumberHelper<T>.Half);
        T cr = T.Cos(x * NumberHelper<T>.Half);
        T sr = T.Sin(x * NumberHelper<T>.Half);

        return new Quaternion<T>(
            sr * cp * cy - cr * sp * sy,
            cr * sp * cy + sr * cp * sy,
            cr * cp * sy - sr * sp * cy,
            cr * cp * cy + sr * sp * sy
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerRadians<T>(Vector3<T> angles)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        return EulerRadians(angles.X, angles.Y, angles.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerDegrees<T>(T x, T y, T z)
        where T : IFloatingPointIeee754<T>
    {
        return EulerRadians(
            FloatingPointIeee754Helper.ConvertDegressToRadians(x),
            FloatingPointIeee754Helper.ConvertDegressToRadians(y),
            FloatingPointIeee754Helper.ConvertDegressToRadians(z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> EulerDegrees<T>(Vector3<T> angles)
        where T : IFloatingPointIeee754<T>
    {
        return EulerDegrees(angles.X, angles.Y, angles.Z);
    }

}
