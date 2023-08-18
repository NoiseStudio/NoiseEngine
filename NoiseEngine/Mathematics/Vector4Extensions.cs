using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class Vector4Extensions {

    /// <summary>
    /// Calculates length of <paramref name="vector"/>.
    /// </summary>
    /// <param name="vector">This <see cref="Vector4{T}"/>.</param>
    /// <returns>Length of <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Magnitude<T>(this Vector4<T> vector) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(vector.MagnitudeSquared());
    }

    /// <summary>
    /// Returns normalized this <paramref name="vector"/>.
    /// </summary>
    /// <param name="vector">This <see cref="Vector4{T}"/>.</param>
    /// <returns>Normalized <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector4<T> Normalize<T>(this Vector4<T> vector) where T : INumber<T>, IFloatingPointIeee754<T> {
        T magnitude = vector.Magnitude();
        return magnitude > T.Epsilon ? (vector / magnitude) : Vector4<T>.Zero;
    }

    /// <summary>
    /// Calculates distance between <paramref name="lhs"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector4{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector4{T}"/>.</param>
    /// <returns>Distance between <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Distance<T>(this Vector4<T> lhs, Vector4<T> rhs) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(lhs.DistanceSquared(rhs));
    }

    /// <summary>
    /// Converts <see cref="Vector4{T}"/> to vector where T is <see cref="float"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector4{T}"/> to convert.</param>
    /// <returns><see cref="Vector4{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 ToFloat(this double4 vector) {
        return new float4((float)vector.X, (float)vector.Y, (float)vector.Z, (float)vector.W);
    }

    /// <summary>
    /// Converts <see cref="Vector4{T}"/> to vector where T is <see cref="float"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector4{T}"/> to convert.</param>
    /// <returns><see cref="Vector4{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float4 ToFloat(this float4 vector) {
        return vector;
    }

    /// <summary>
    /// Converts <see cref="Vector4{T}"/> to vector where T is <see cref="double"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector4{T}"/> to convert.</param>
    /// <returns><see cref="Vector4{T}"/> with <see cref="double"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double4 ToDouble(this float4 vector) {
        return new double4(vector.X, vector.Y, vector.Z, vector.W);
    }

    /// <summary>
    /// Converts <see cref="Vector4{T}"/> to vector where T is pos.
    /// </summary>
    /// <param name="vector"><see cref="Vector4{T}"/> to convert.</param>
    /// <returns><see cref="Vector4{T}"/> with pos components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static pos4 ToPos(this float4 vector) {
#if NE_LARGE_WORLD
        return vector.ToDouble();
#else
        return vector;
#endif
    }

}
