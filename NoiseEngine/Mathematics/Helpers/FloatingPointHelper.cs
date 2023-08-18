using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics.Helpers;

internal static class FloatingPointHelper<T> where T : IFloatingPoint<T> {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetExponentByteCount(T value) {
        return value.GetExponentByteCount();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetExponentShortestBitLength(T value) {
        return value.GetExponentShortestBitLength();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSignificandBitLength(T value) {
        return value.GetSignificandBitLength();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetSignificandByteCount(T value) {
        return value.GetSignificandByteCount();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteExponentBigEndian(T value, Span<byte> destination, out int bytesWritten) {
        return value.TryWriteExponentBigEndian(destination, out bytesWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteExponentLittleEndian(T value, Span<byte> destination, out int bytesWritten) {
        return value.TryWriteExponentLittleEndian(destination, out bytesWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteSignificandBigEndian(T value, Span<byte> destination, out int bytesWritten) {
        return value.TryWriteSignificandBigEndian(destination, out bytesWritten);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryWriteSignificandLittleEndian(T value, Span<byte> destination, out int bytesWritten) {
        return value.TryWriteSignificandLittleEndian(destination, out bytesWritten);
    }

}
