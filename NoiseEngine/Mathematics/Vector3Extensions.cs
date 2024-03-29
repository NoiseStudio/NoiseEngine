﻿using NoiseEngine.Mathematics.Helpers;
using System;
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

    /// <summary>
    /// Converts <see cref="Vector3{T}"/> to vector where T is <see cref="float"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector3{T}"/> to convert.</param>
    /// <returns><see cref="Vector3{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToFloat<T>(this Vector3<T> vector) where T : IConvertible, INumber<T> {
        return new float3(
            VectorHelper.ToFloat(vector.X), VectorHelper.ToFloat(vector.Y), VectorHelper.ToFloat(vector.Z)
        );
    }

    /// <summary>
    /// Converts <see cref="Vector3{T}"/> to vector where T is <see cref="double"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector3{T}"/> to convert.</param>
    /// <returns><see cref="Vector3{T}"/> with <see cref="double"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double3 ToDouble<T>(this Vector3<T> vector) where T : IConvertible, INumber<T> {
        return new double3(
            VectorHelper.ToDouble(vector.X), VectorHelper.ToDouble(vector.Y), VectorHelper.ToDouble(vector.Z)
        );
    }

    /// <summary>
    /// Converts <see cref="Vector3{T}"/> to vector where T is <see cref="Position"/>.
    /// </summary>
    /// <param name="vector"><see cref="Vector3{T}"/> to convert.</param>
    /// <returns><see cref="Vector3{T}"/> with <see cref="Position"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static pos3 ToPos<T>(this Vector3<T> vector) where T : IConvertible, INumber<T> {
        return new pos3(
            VectorHelper.ToPos(vector.X), VectorHelper.ToPos(vector.Y), VectorHelper.ToPos(vector.Z)
        );
    }

}
