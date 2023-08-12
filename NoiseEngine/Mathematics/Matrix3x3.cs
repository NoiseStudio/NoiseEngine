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
    public Matrix3x3(T m11, T m21, T m31, T m12, T m22, T m32, T m13, T m23, T m33) : this(
        new Vector3<T>(m11, m21, m31), new Vector3<T>(m12, m22, m32), new Vector3<T>(m13, m23, m33)
    ) {
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
