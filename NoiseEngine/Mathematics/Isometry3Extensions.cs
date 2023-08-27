using NoiseEngine.Mathematics.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NoiseEngine.Mathematics;

internal static class Isometry3Extensions {

    /// <summary>
    /// Converts <see cref="Isometry3{T}"/> to vector where T is <see cref="float"/>.
    /// </summary>
    /// <param name="isometry"><see cref="Isometry3{T}"/> to convert.</param>
    /// <returns><see cref="Isometry3{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Isometry3<float> ToFloat<T>(this Isometry3<T> isometry) where T : IConvertible, INumber<T> {
        return new Isometry3<float>(isometry.Translation.ToFloat(), isometry.Rotation.ToFloat());
    }

    /// <summary>
    /// Converts <see cref="Isometry3{T}"/> to vector where T is <see cref="double"/>.
    /// </summary>
    /// <param name="isometry"><see cref="Isometry3{T}"/> to convert.</param>
    /// <returns><see cref="Isometry3{T}"/> with <see cref="double"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Isometry3<double> ToDouble<T>(this Isometry3<T> isometry) where T : IConvertible, INumber<T> {
        return new Isometry3<double>(isometry.Translation.ToDouble(), isometry.Rotation.ToDouble());
    }

    /// <summary>
    /// Converts <see cref="Isometry3{T}"/> to vector where T is <see cref="Position"/>.
    /// </summary>
    /// <param name="isometry"><see cref="Isometry3{T}"/> to convert.</param>
    /// <returns><see cref="Isometry3{T}"/> with <see cref="Position"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Isometry3<pos> ToPos<T>(this Isometry3<T> isometry) where T : IConvertible, INumber<T> {
        return new Isometry3<pos>(isometry.Translation.ToPos(), isometry.Rotation.ToPos());
    }

}
