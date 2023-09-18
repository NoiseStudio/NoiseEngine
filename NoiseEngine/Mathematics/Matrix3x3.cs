using NoiseEngine.Mathematics.Helpers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Mathematics;

// Matrix layout:
//
//     |  C1   C2   C3
//  ---+----------------
//  ---| C1.X C2.X C3.X
//  ---| C1.Y C2.Y C3.Y
//  ---| C1.Z C2.Z C3.Z

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Matrix3x3<T>(Vector3<T> C1, Vector3<T> C2, Vector3<T> C3)
    where T : INumber<T>
{

    public T M11 => C1.X;
    public T M12 => C2.X;
    public T M13 => C3.X;
    public T M21 => C1.Y;
    public T M22 => C2.Y;
    public T M23 => C3.Y;
    public T M31 => C1.Z;
    public T M32 => C2.Z;
    public T M33 => C3.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Matrix3x3(T m11, T m21, T m31, T m12, T m22, T m32, T m13, T m23, T m33) : this(
        new Vector3<T>(m11, m21, m31), new Vector3<T>(m12, m22, m32), new Vector3<T>(m13, m23, m33)
    ) {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x3<T> Rotate(Quaternion<T> quaternion) {
        T x = quaternion.X * NumberHelper<T>.Value2;
        T y = quaternion.Y * NumberHelper<T>.Value2;
        T z = quaternion.Z * NumberHelper<T>.Value2;
        T xx = quaternion.X * x;
        T yy = quaternion.Y * y;
        T zz = quaternion.Z * z;
        T xy = quaternion.X * y;
        T xz = quaternion.X * z;
        T yz = quaternion.Y * z;
        T wx = quaternion.W * x;
        T wy = quaternion.W * y;
        T wz = quaternion.W * z;

        return new Matrix3x3<T>(
            new Vector3<T>(T.One - (yy + zz), xy + wz, xz - wy),
            new Vector3<T>(xy - wz, T.One - (xx + zz), yz + wx),
            new Vector3<T>(xz + wy, yz - wx, T.One - (xx + yy))
        );
    }

    /// <summary>
    /// Creates transpose <see cref="Matrix3x3{T}"/> of this <see cref="Matrix3x3{T}"/>.
    /// </summary>
    /// <returns>Transposed <see cref="Matrix3x3{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x3<T> Transpose() {
        return new Matrix3x3<T>(
            M11, M12, M13,
            M21, M22, M23,
            M31, M32, M33
        );
    }

    /// <summary>
    /// Calculates the determinant of this <see cref="Matrix3x3{T}"/>.
    /// </summary>
    /// <returns>The determinant.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Determinant() {
        return M11 * (M22 * M33 - M32 * M23) -
               M12 * (M21 * M33 - M23 * M31) +
               M13 * (M21 * M32 - M22 * M31);
    }

    /// <summary>
    /// Try inverts this <see cref="Matrix3x3{T}"/>.
    /// </summary>
    /// <param name="invertedMatrix">
    /// When this method returns, contains the inverted matrix if the operation succeeded.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if this <see cref="Matrix3x3{T}"/> was inverted successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryInvert(out Matrix3x3<T> invertedMatrix) {
        T determinant = Determinant();
        if (determinant == T.Zero) {
            invertedMatrix = default;
            return false;
        }

        T i = T.One / determinant;
        invertedMatrix = new Matrix3x3<T>(
            (M22 * M33 - M32 * M23) * i,
            (M23 * M31 - M21 * M33) * i,
            (M21 * M32 - M22 * M31) * i,
            (M13 * M32 - M12 * M33) * i,
            (M11 * M33 - M13 * M31) * i,
            (M12 * M31 - M11 * M32) * i,
            (M12 * M23 - M13 * M22) * i,
            (M13 * M21 - M11 * M23) * i,
            (M11 * M22 - M12 * M21) * i
        );
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix3x3<T> operator *(Matrix3x3<T> lhs, Matrix3x3<T> rhs) {
        return new Matrix3x3<T>(
            (lhs.M11 * rhs.M11) + (lhs.M12 * rhs.M21) + (lhs.M13 * rhs.M31),
            (lhs.M21 * rhs.M11) + (lhs.M22 * rhs.M21) + (lhs.M23 * rhs.M31),
            (lhs.M31 * rhs.M11) + (lhs.M32 * rhs.M21) + (lhs.M33 * rhs.M31),
            (lhs.M11 * rhs.M12) + (lhs.M12 * rhs.M22) + (lhs.M13 * rhs.M32),
            (lhs.M21 * rhs.M12) + (lhs.M22 * rhs.M22) + (lhs.M23 * rhs.M32),
            (lhs.M31 * rhs.M12) + (lhs.M32 * rhs.M22) + (lhs.M33 * rhs.M32),
            (lhs.M11 * rhs.M13) + (lhs.M12 * rhs.M23) + (lhs.M13 * rhs.M33),
            (lhs.M21 * rhs.M13) + (lhs.M22 * rhs.M23) + (lhs.M23 * rhs.M33),
            (lhs.M31 * rhs.M13) + (lhs.M32 * rhs.M23) + (lhs.M33 * rhs.M33)
        );
    }

    /// <summary>
    /// Returns the <paramref name="vector"/> multiplied by the <paramref name="lhs"/>.
    /// </summary>
    /// <param name="lhs"><see cref="Matrix3x3{T}"/> to multiplication.</param>
    /// <param name="vector"><see cref="Vector3{T}"/> to multiplication.</param>
    /// <returns>A <paramref name="vector"/> multiplied by the <paramref name="lhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator *(Matrix3x3<T> lhs, Vector3<T> vector) {
        return new Vector3<T>(
            lhs.C1.X * vector.X + lhs.C2.X * vector.Y + lhs.C3.X * vector.Z,
            lhs.C1.Y * vector.X + lhs.C2.Y * vector.Y + lhs.C3.Y * vector.Z,
            lhs.C1.Z * vector.X + lhs.C2.Z * vector.Y + lhs.C3.Z * vector.Z
        );
    }

}
