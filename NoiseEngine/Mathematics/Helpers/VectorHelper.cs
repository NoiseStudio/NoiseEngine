using System;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics.Helpers;

internal static class VectorHelper {

    // TODO: Optimize conversion methods, like float to int.
    // NOTE: Conversion methods is wrapped to easier optimization in different cases.

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToFloat<T>(T value) where T : IConvertible {
        return value.ToSingle(null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ToDouble<T>(T value) where T : IConvertible {
        return value.ToDouble(null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static pos ToPos<T>(T value) where T : IConvertible {
#if NE_LARGE_WORLD
        return (pos)ToDouble(value);
#else
        return ToFloat(value);
#endif
    }

}
