using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Mathematics;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Quaternion<T>(T X, T Y, T Z, T W) where T : INumber<T> {

    public static Quaternion<T> Identity => new Quaternion<T>(T.Zero, T.Zero, T.Zero, T.One);

    /// <summary>
    /// Calculates dot product of this <see cref="Quaternion{T}"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="rhs">Second <see cref="Quaternion{T}"/>.</param>
    /// <returns>Dot product of this <see cref="Quaternion{T}"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Quaternion<T> rhs) {
        return X * rhs.X + Y * rhs.Y + Z * rhs.Z + W * rhs.W;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> operator *(Quaternion<T> lhs, Quaternion<T> rhs) {
        return new Quaternion<T>(
            lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
            lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
            lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
            lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator *(Quaternion<T> rotation, Vector3<T> point) {
        T x = rotation.X * NumberHelper<T>.Two;
        T y = rotation.Y * NumberHelper<T>.Two;
        T z = rotation.Z * NumberHelper<T>.Two;
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

}
