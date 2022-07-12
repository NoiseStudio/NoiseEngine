using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public readonly record struct Vector3<T>(T X, T Y, T Z) where T : INumber<T> {

    public static Vector3<T> Zero => new Vector3<T>(T.Zero, T.Zero, T.Zero);
    public static Vector3<T> One => new Vector3<T>(T.One, T.One, T.One);

    public static Vector3<T> Left => new Vector3<T>(-T.One, T.Zero, T.Zero);
    public static Vector3<T> Right => new Vector3<T>(T.One, T.Zero, T.Zero);
    public static Vector3<T> Up => new Vector3<T>(T.Zero, T.One, T.Zero);
    public static Vector3<T> Down => new Vector3<T>(T.Zero, -T.One, T.Zero);
    public static Vector3<T> Front => new Vector3<T>(T.Zero, T.Zero, T.One);
    public static Vector3<T> Back => new Vector3<T>(T.Zero, T.Zero, -T.One);

    public Vector3(T x) : this(x, T.Zero, T.Zero) {
    }

    public Vector3(T x, T y) : this(x, y, T.Zero) {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T MagnitudeSquared() {
        return X * X + Y * Y + Z * Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dot(Vector3<T> rhs) {
        return X * rhs.X + Y * rhs.Y + Z * rhs.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T DistanceSquared(Vector3<T> rhs) {
        return (this - rhs).MagnitudeSquared();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> Scale(Vector3<T> rhs) {
        return new Vector3<T>(X * rhs.X, Y * rhs.Y, Z * rhs.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> Lerp(Vector3<T> rhs, T t) {
        return this + (rhs - this) * t;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> Cross(Vector3<T> rhs) {
        return new Vector3<T>(
            Y * rhs.Z - Z * rhs.Y,
            Z * rhs.X - X * rhs.Z,
            X * rhs.Y - Y * rhs.X
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator +(Vector3<T> lhs, Vector3<T> rhs) {
        return new Vector3<T>(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator -(Vector3<T> lhs, Vector3<T> rhs) {
        return new Vector3<T>(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator *(Vector3<T> lhs, T rhs) {
        return new Vector3<T>(lhs.X * rhs, lhs.Y * rhs, lhs.Z * rhs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator /(Vector3<T> lhs, T rhs) {
        return new Vector3<T>(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3<T>(T value) {
        return new Vector3<T>(value);
    }

}
