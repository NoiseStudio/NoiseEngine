using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics.Helpers;

internal static class BitwiseOperatorsHelper<TSelf, TOther, TResult>
    where TSelf : IBitwiseOperators<TSelf, TOther, TResult>
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult BitwiseAnd(TSelf left, TOther right) {
        return left & right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult BitwiseOr(TSelf left, TOther right) {
        return left | right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult ExclusiveOr(TSelf left, TOther right) {
        return left ^ right;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TResult OnesComplement(TSelf value) {
        return ~value;
    }

}
