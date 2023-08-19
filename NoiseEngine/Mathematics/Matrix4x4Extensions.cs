using System.Numerics;
using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class Matrix4x4Extensions {

    /// <summary>
    /// Converts <see cref="Matrix4x4{T}"/> to matrix where T is <see cref="float"/>.
    /// </summary>
    /// <param name="matrix"><see cref="Matrix4x4{T}"/> to convert.</param>
    /// <returns><see cref="Matrix4x4{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<float> ToFloat<T>(this Matrix4x4<T> matrix) where T : IConvertible, INumber<T> {
        return new Matrix4x4<float>(matrix.C0.ToFloat(), matrix.C1.ToFloat(), matrix.C2.ToFloat(), matrix.C3.ToFloat());
    }

    /// <summary>
    /// Converts <see cref="Matrix4x4{T}"/> to matrix where T is <see cref="double"/>.
    /// </summary>
    /// <param name="matrix"><see cref="Matrix4x4{T}"/> to convert.</param>
    /// <returns><see cref="Matrix4x4{T}"/> with <see cref="double"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<double> ToDouble<T>(this Matrix4x4<T> matrix) where T : IConvertible, INumber<T> {
        return new Matrix4x4<double>(
            matrix.C0.ToDouble(), matrix.C1.ToDouble(), matrix.C2.ToDouble(), matrix.C3.ToDouble()
        );
    }

    /// <summary>
    /// Converts <see cref="Matrix4x4{T}"/> to matrix where T is <see cref="Position"/>.
    /// </summary>
    /// <param name="matrix"><see cref="Matrix4x4{T}"/> to convert.</param>
    /// <returns><see cref="Matrix4x4{T}"/> with <see cref="Position"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<pos> ToPos<T>(this Matrix4x4<T> matrix) where T : IConvertible, INumber<T> {
        return new Matrix4x4<pos>(matrix.C0.ToPos(), matrix.C1.ToPos(), matrix.C2.ToPos(), matrix.C3.ToPos());
    }

}
