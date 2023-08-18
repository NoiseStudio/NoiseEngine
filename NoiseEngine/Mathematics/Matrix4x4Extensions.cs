using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class Matrix4x4Extensions {

    /// <summary>
    /// Converts <see cref="Matrix4x4{T}"/> to matrix where T is <see cref="float"/>.
    /// </summary>
    /// <param name="matrix"><see cref="Matrix4x4{T}"/> to convert.</param>
    /// <returns><see cref="Matrix4x4{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<float> ToFloat(this Matrix4x4<float> matrix) {
        return matrix;
    }

    /// <summary>
    /// Converts <see cref="Matrix4x4{T}"/> to matrix where T is <see cref="float"/>.
    /// </summary>
    /// <param name="matrix"><see cref="Matrix4x4{T}"/> to convert.</param>
    /// <returns><see cref="Matrix4x4{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4<float> ToFloat(this Matrix4x4<double> matrix) {
        return new Matrix4x4<float>(matrix.C0.ToFloat(), matrix.C1.ToFloat(), matrix.C2.ToFloat(), matrix.C3.ToFloat());
    }

}
