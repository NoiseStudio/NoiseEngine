using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class Vector2Extensions {

    /// <summary>
    /// Calculates length of <paramref name="vector"/>.
    /// </summary>
    /// <param name="vector">This <see cref="Vector2{T}"/>.</param>
    /// <returns>Length of <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Magnitude<T>(this Vector2<T> vector) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(vector.MagnitudeSquared());
    }

    /// <summary>
    /// Returns normalized this <paramref name="vector"/>.
    /// </summary>
    /// <param name="vector">This <see cref="Vector2{T}"/>.</param>
    /// <returns>Normalized <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2<T> Normalize<T>(this Vector2<T> vector) where T : INumber<T>, IFloatingPointIeee754<T> {
        T magnitude = vector.Magnitude();
        return magnitude > T.Epsilon ? (vector / magnitude) : Vector2<T>.Zero;
    }

    /// <summary>
    /// Calculates distance between <paramref name="lhs"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector2{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector2{T}"/>.</param>
    /// <returns>Distance between <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Distance<T>(this Vector2<T> lhs, Vector2<T> rhs) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(lhs.DistanceSquared(rhs));
    }

}
