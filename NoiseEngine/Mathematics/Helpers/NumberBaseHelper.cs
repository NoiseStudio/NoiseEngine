using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics.Helpers;

internal static class NumberBaseHelper<T> where T : INumberBase<T> {

    public static int Radix => T.Radix;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEvenInteger(T value) {
        return T.IsEvenInteger(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(T value) {
        return T.IsFinite(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInfinity(T value) {
        return T.IsInfinity(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsInteger(T value) {
        return T.IsInteger(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaN(T value) {
        return T.IsNaN(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegative(T value) {
        return T.IsNegative(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNegativeInfinity(T value) {
        return T.IsNegativeInfinity(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNormal(T value) {
        return T.IsNormal(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOddInteger(T value) {
        return T.IsOddInteger(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositive(T value) {
        return T.IsPositive(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPositiveInfinity(T value) {
        return T.IsPositiveInfinity(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSubnormal(T value) {
        return T.IsSubnormal(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsRealNumber(T value) {
        return T.IsRealNumber(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MaxMagnitude(T x, T y) {
        return T.MaxMagnitude(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MaxMagnitudeNumber(T x, T y) {
        return T.MaxMagnitudeNumber(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MinMagnitude(T x, T y) {
        return T.MinMagnitude(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T MinMagnitudeNumber(T x, T y) {
        return T.MinMagnitudeNumber(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCanonical(T value) {
        return T.IsCanonical(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsComplexNumber(T value) {
        return T.IsComplexNumber(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsImaginaryNumber(T value) {
        return T.IsImaginaryNumber(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZero(T value) {
        return T.IsZero(value);
    }

}
