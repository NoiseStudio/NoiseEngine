﻿using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

internal static class FloatingPointIeee754Helper {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConvertRadiansToDegrees<T>(T radians) where T : IFloatingPointIeee754<T> {
        return radians * NumberHelper<T>.Value180 / T.Pi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConvertDegreesToRadians<T>(T degrees) where T : IFloatingPointIeee754<T> {
        return degrees * T.Pi / NumberHelper<T>.Value180;
    }

}