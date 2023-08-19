using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics.Helpers;

internal static class FloatingPointIeee754Helper<T> where T : IFloatingPointIeee754<T> {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConvertRadiansToDegrees(T radians) {
        return radians * NumberHelper<T>.Value180 / T.Pi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ConvertDegreesToRadians(T degrees) {
        return degrees * T.Pi / NumberHelper<T>.Value180;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Ieee754Remainder(T left, T right) {
        return T.Ieee754Remainder(left, right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Atan2(T y, T x) {
        return T.Atan2(y, x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Atan2Pi(T y, T x) {
        return T.Atan2Pi(y, x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BitDecrement(T x) {
        return T.BitDecrement(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T BitIncrement(T x) {
        return T.BitIncrement(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T FusedMultiplyAdd(T left, T right, T addend) {
        return T.FusedMultiplyAdd(left, right, addend);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ILogB(T x) {
        return T.ILogB(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ScaleB(T x, int n) {
        return T.ScaleB(x, n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp(T x) {
        return T.Exp(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp10(T x) {
        return T.Exp10(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Exp2(T x) {
        return T.Exp2(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Round(T x, int digits, MidpointRounding mode) {
        return T.Round(x, digits, mode);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Acosh(T x) {
        return T.Acosh(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Asinh(T x) {
        return T.Asinh(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Atanh(T x) {
        return T.Atanh(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cosh(T x) {
        return T.Cosh(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sinh(T x) {
        return T.Sinh(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Tanh(T x) {
        return T.Tanh(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log(T x) {
        return T.Log(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log(T x, T newBase) {
        return T.Log(x, newBase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Log10(T x) {
        return T.Log10(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Pow(T x, T y) {
        return T.Pow(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cbrt(T x) {
        return T.Cbrt(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Hypot(T x, T y) {
        return T.Hypot(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T RootN(T x, int n) {
        return T.RootN(x, n);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sqrt(T x) {
        return T.Sqrt(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Acos(T x) {
        return T.Acos(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AcosPi(T x) {
        return T.AcosPi(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Asin(T x) {
        return T.Asin(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AsinPi(T x) {
        return T.AsinPi(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Atan(T x) {
        return T.Atan(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AtanPi(T x) {
        return T.AtanPi(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cos(T x) {
        return T.Cos(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CosPi(T x) {
        return T.CosPi(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Sin(T x) {
        return T.Sin(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T Sin, T Cos) SinCos(T x) {
        return T.SinCos(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (T SinPi, T CosPi) SinCosPi(T x) {
        return T.SinCosPi(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T SinPi(T x) {
        return T.SinPi(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Tan(T x) {
        return T.Tan(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T TanPi(T x) {
        return T.TanPi(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Abs(T value) {
        return T.Abs(value);
    }

}
