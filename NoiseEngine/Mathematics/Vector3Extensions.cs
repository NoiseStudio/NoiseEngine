using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class Vector3Extensions {

    /// <summary>
    /// Calculates length of <paramref name="vector"/>.
    /// </summary>
    /// <param name="vector">This <see cref="Vector3{T}"/>.</param>
    /// <returns>Length of <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Magnitude<T>(this Vector3<T> vector) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(vector.MagnitudeSquared());
    }

    /// <summary>
    /// Returns normalized this <paramref name="vector"/>.
    /// </summary>
    /// <param name="vector">This <see cref="Vector3{T}"/>.</param>
    /// <returns>Normalized <paramref name="vector"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> Normalize<T>(this Vector3<T> vector) where T : INumber<T>, IFloatingPointIeee754<T> {
        T magnitude = vector.Magnitude();
        return magnitude > T.Epsilon ? (vector / magnitude) : Vector3<T>.Zero;
    }

    /// <summary>
    /// Calculates distance between <paramref name="lhs"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Vector3{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Vector3{T}"/>.</param>
    /// <returns>Distance between <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Distance<T>(this Vector3<T> lhs, Vector3<T> rhs) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(lhs.DistanceSquared(rhs));
    }

}
