using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NoiseEngine.Mathematics.Helpers;

namespace NoiseEngine.Mathematics;

// Matrix layout:
//
//     |  C0   C1   C2   C3
//  ---+---------------------
//  ---| C0.X C1.X C2.X C3.X
//  ---| C0.Y C1.Y C2.Y C3.Y
//  ---| C0.Z C1.Z C2.Z C3.Z
//  ---| C0.W C1.W C2.W C3.W

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Matrix4x4<T>(Vector4<T> C0, Vector4<T> C1, Vector4<T> C2, Vector4<T> C3)
    where T : INumber<T>
{

    public static Matrix4x4<T> Identity =>
        new Matrix4x4<T>(Vector4<T>.Right, Vector4<T>.Up, Vector4<T>.Front, Vector4<T>.Ana);

    /// <summary>
    /// Creates a scaling <see cref="Matrix4x4{T}"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector3{T}"/> from which scaled
    /// <see cref="Matrix4x4{T}"/> will be created.</param>
    /// <returns>Return <see cref="Matrix4x4{T}"/> scaled along
    /// coordinate axes by a <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<T> Scale(Vector3<T> vector) {
        return new Matrix4x4<T>(
            new Vector4<T>(vector.X, T.Zero, T.Zero, T.Zero),
            new Vector4<T>(T.Zero, vector.Y, T.Zero, T.Zero),
            new Vector4<T>(T.Zero, T.Zero, vector.Z, T.Zero),
            Vector4<T>.Ana
        );
    }

    /// <summary>
    /// Creates a translation <see cref="Matrix4x4{T}"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector3{T}"/> from which translation
    /// <see cref="Matrix4x4{T}"/> will be created.</param>
    /// <returns>Return translation <see cref="Matrix4x4{T}"/> from <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<T> Translate(Vector3<T> vector) {
        return new Matrix4x4<T>(
            Vector4<T>.Right,
            Vector4<T>.Up,
            Vector4<T>.Front,
            new Vector4<T>(vector.X, vector.Y, vector.Z, T.One)
        );
    }

    /// <summary>
    /// Creates a rotation <see cref="Matrix4x4{T}"/>.
    /// </summary>
    /// <param name="quaternion"><see cref="Quaternion{T}"/> from which rotation
    /// <see cref="Matrix4x4{T}"/> will be created.</param>
    /// <returns>Return rotation <see cref="Matrix4x4{T}"/> from <paramref name="quaternion"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<T> Rotate(Quaternion<T> quaternion) {
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

        return new Matrix4x4<T>(
            new Vector4<T>(T.One - (yy + zz), xy + wz, xz - wy, T.Zero),
            new Vector4<T>(xy - wz, T.One - (xx + zz), yz + wx, T.Zero),
            new Vector4<T>(xz + wy, yz - wx, T.One - (xx + yy), T.Zero),
            Vector4<T>.Ana
        );
    }

    /// <summary>
    /// Creates a orthographic projection <see cref="Matrix4x4{T}"/>.
    /// </summary>
    /// <param name="right">The maximum X-value of the view volume.</param>
    /// <param name="left">The minimum X-value of the view volume.</param>
    /// <param name="top">The maximum Y-value of the view volume.</param>
    /// <param name="bottom">The minimum Y-value of the view volume.</param>
    /// <param name="near">The minimum Z-value of the view volume.</param>
    /// <param name="far">The maximum Z-value of the view volume.</param>
    /// <returns>The orthographic projection <see cref="Matrix4x4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<T> OrthographicProjection(T right, T left, T top, T bottom, T near, T far) {
        return new Matrix4x4<T>(
            new Vector4<T>(NumberHelper<T>.Value2 / (right - left), T.Zero, T.Zero, T.Zero),
            new Vector4<T>(T.Zero, NumberHelper<T>.Value2 / (top - bottom), T.Zero, T.Zero),
            new Vector4<T>(T.Zero, T.Zero, -NumberHelper<T>.Value2 / (far - near), T.Zero),
            new Vector4<T>(
                -(right + left) / (right - left),
                -(top + bottom) / (top - bottom),
                -(far + near) / (far - near),
                T.One
            )
        );
    }

    /// <summary>
    /// Creates a perspective projection <see cref="Matrix4x4{T}"/>.
    /// </summary>
    /// <param name="right">The maximum X-value of the view volume at the near view plane.</param>
    /// <param name="left">The minimum X-value of the view volume at the near view plane.</param>
    /// <param name="top">The maximum Y-value of the view volume at the near view plane.</param>
    /// <param name="bottom">The minimum Y-value of the view volume at the near view plane.</param>
    /// <param name="near">The distance to the near view plane.</param>
    /// <param name="far">The distance to the far view plane.</param>
    /// <returns>The perspective projection <see cref="Matrix4x4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<T> PerspectiveProjection(T right, T left, T top, T bottom, T near, T far) {
        return new Matrix4x4<T>(
            new Vector4<T>(NumberHelper<T>.Value2 * near / (right - left), T.Zero, T.Zero, T.Zero),
            new Vector4<T>(T.Zero, NumberHelper<T>.Value2 * near / (top - bottom), T.Zero, T.Zero),
            new Vector4<T>(
                (right + left) / (right - left),
                (top + bottom) / (top - bottom),
                -far / (far - near),
                -T.One
            ),
            new Vector4<T>(T.Zero, T.Zero, -far * near / (far - near), T.Zero)
        );
    }

    /// <summary>
    /// Returns transformed <paramref name="point"/> by this <see cref="Matrix4x4{T}"/>, with a perspective divide.
    /// </summary>
    /// <param name="point"><see cref="Vector3{T}"/> to be used for the transformation.</param>
    /// <returns>Returns a <paramref name="point"/> transformed by the
    /// current fully arbitrary <see cref="Matrix4x4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> MultiplyPoint(Vector3<T> point) {
        T w = C0.W * point.X + C1.W * point.Y + C2.W * point.Z + C3.W;
        return MultiplyPoint3x4(point) * (T.One / w);
    }

    /// <summary>
    /// Returns transformed <paramref name="point"/> by this <see cref="Matrix4x4{T}"/>, without a perspective divide.
    /// </summary>
    /// <param name="point"><see cref="Vector3{T}"/> to be used for the transformation.</param>
    /// <returns>Returns a <paramref name="point"/> transformed by the
    /// current fully arbitrary <see cref="Matrix4x4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> MultiplyPoint3x4(Vector3<T> point) {
        return MultiplyVector(point) + new Vector3<T>(C3.X, C3.Y, C3.Z);
    }

    /// <summary>
    /// Returns transformed <paramref name="vector"/> by this <see cref="Matrix4x4{T}"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector3{T}"/> to be used for the transformation.</param>
    /// <returns>Returns a <paramref name="vector"/> transformed by the
    /// current direction of <see cref="Matrix4x4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> MultiplyVector(Vector3<T> vector) {
        return new Vector3<T>(
            C0.X * vector.X + C1.X * vector.Y + C2.X * vector.Z,
            C0.Y * vector.X + C1.Y * vector.Y + C2.Y * vector.Z,
            C0.Z * vector.X + C1.Z * vector.Y + C2.Z * vector.Z
        );
    }

    /// <summary>
    /// Returns the <see cref="Matrix4x4{T}"/> that results from scaling all the elements
    /// of a specified <see cref="Matrix4x4{T}"/> by a scalar factor.
    /// </summary>
    /// <param name="lhs"><see cref="Matrix4x4{T}"/> to scale.</param>
    /// <param name="vector">Scaling <see cref="Vector4{T}"/> to use.</param>
    /// <returns>Scaled <see cref="Matrix4x4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4<T> operator *(Matrix4x4<T> lhs, Vector4<T> vector) {
        return new Vector4<T>(
            lhs.C0.X * vector.X + lhs.C1.X * vector.Y + lhs.C2.X * vector.Z + lhs.C3.X * vector.W,
            lhs.C0.Y * vector.X + lhs.C1.Y * vector.Y + lhs.C2.Y * vector.Z + lhs.C3.Y * vector.W,
            lhs.C0.Z * vector.X + lhs.C1.Z * vector.Y + lhs.C2.Z * vector.Z + lhs.C3.Z * vector.W,
            lhs.C0.W * vector.X + lhs.C1.W * vector.Y + lhs.C2.W * vector.Z + lhs.C3.W * vector.W
        );
    }

    /// <summary>
    /// Returns the <see cref="Matrix4x4{T}"/> that results from multiplying two matrices together.
    /// </summary>
    /// <param name="lhs">First <see cref="Matrix4x4{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Matrix4x4{T}"/>.</param>
    /// <returns>Product <see cref="Matrix4x4{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<T> operator *(Matrix4x4<T> lhs, Matrix4x4<T> rhs) {
        return new Matrix4x4<T>(
            new Vector4<T>(
                lhs.C0.X * rhs.C0.X + lhs.C1.X * rhs.C0.Y + lhs.C2.X * rhs.C0.Z + lhs.C3.X * rhs.C0.W,
                lhs.C0.Y * rhs.C0.X + lhs.C1.Y * rhs.C0.Y + lhs.C2.Y * rhs.C0.Z + lhs.C3.Y * rhs.C0.W,
                lhs.C0.Z * rhs.C0.X + lhs.C1.Z * rhs.C0.Y + lhs.C2.Z * rhs.C0.Z + lhs.C3.Z * rhs.C0.W,
                lhs.C0.W * rhs.C0.X + lhs.C1.W * rhs.C0.Y + lhs.C2.W * rhs.C0.Z + lhs.C3.W * rhs.C0.W
            ),
            new Vector4<T>(
                lhs.C0.X * rhs.C1.X + lhs.C1.X * rhs.C1.Y + lhs.C2.X * rhs.C1.Z + lhs.C3.X * rhs.C1.W,
                lhs.C0.Y * rhs.C1.X + lhs.C1.Y * rhs.C1.Y + lhs.C2.Y * rhs.C1.Z + lhs.C3.Y * rhs.C1.W,
                lhs.C0.Z * rhs.C1.X + lhs.C1.Z * rhs.C1.Y + lhs.C2.Z * rhs.C1.Z + lhs.C3.Z * rhs.C1.W,
                lhs.C0.W * rhs.C1.X + lhs.C1.W * rhs.C1.Y + lhs.C2.W * rhs.C1.Z + lhs.C3.W * rhs.C1.W
            ),
            new Vector4<T>(
                lhs.C0.X * rhs.C2.X + lhs.C1.X * rhs.C2.Y + lhs.C2.X * rhs.C2.Z + lhs.C3.X * rhs.C2.W,
                lhs.C0.Y * rhs.C2.X + lhs.C1.Y * rhs.C2.Y + lhs.C2.Y * rhs.C2.Z + lhs.C3.Y * rhs.C2.W,
                lhs.C0.Z * rhs.C2.X + lhs.C1.Z * rhs.C2.Y + lhs.C2.Z * rhs.C2.Z + lhs.C3.Z * rhs.C2.W,
                lhs.C0.W * rhs.C2.X + lhs.C1.W * rhs.C2.Y + lhs.C2.W * rhs.C2.Z + lhs.C3.W * rhs.C2.W
            ),
            new Vector4<T>(
                lhs.C0.X * rhs.C3.X + lhs.C1.X * rhs.C3.Y + lhs.C2.X * rhs.C3.Z + lhs.C3.X * rhs.C3.W,
                lhs.C0.Y * rhs.C3.X + lhs.C1.Y * rhs.C3.Y + lhs.C2.Y * rhs.C3.Z + lhs.C3.Y * rhs.C3.W,
                lhs.C0.Z * rhs.C3.X + lhs.C1.Z * rhs.C3.Y + lhs.C2.Z * rhs.C3.Z + lhs.C3.Z * rhs.C3.W,
                lhs.C0.W * rhs.C3.X + lhs.C1.W * rhs.C3.Y + lhs.C2.W * rhs.C3.Z + lhs.C3.W * rhs.C3.W
            )
        );
    }

}
