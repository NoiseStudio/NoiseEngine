using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics.Helpers;

internal static class BinaryNumberHelper<T> where T : IBinaryNumber<T> {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPow2(T value) {
        return T.IsPow2(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log2(T value) {
        return T.Log2(value);
    }

}
