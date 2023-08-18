using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using NoiseEngine.Mathematics.Helpers;

namespace NoiseEngine.Mathematics;

public static class QuaternionExtensions {

    /// <summary>
    /// Converts <paramref name="quaternion"/> to <see cref="Vector3{T}"/> with euler angles in radians.
    /// </summary>
    /// <param name="quaternion"><see cref="Quaternion{T}"/> to convert.</param>
    /// <returns>Euler angles of <paramref name="quaternion"/> in radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> ToEulerRadians<T>(this Quaternion<T> quaternion)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        T t0 = (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z) * NumberHelper<T>.Value2;
        T t1 = T.One - (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y) * NumberHelper<T>.Value2;

        T t2 = (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X) * NumberHelper<T>.Value2;
        t2 = T.Min(T.One, t2);
        t2 = t2 < -T.One ? T.One : t2;

        T t3 = (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y) * NumberHelper<T>.Value2;
        T t4 = T.One - (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z) * NumberHelper<T>.Value2;

        return new Vector3<T>(
            TrigonometricFunctionsHelper.Atan2(t0, t1),T.Asin(t2), TrigonometricFunctionsHelper.Atan2(t3, t4)
        );
    }

    /// <summary>
    /// Converts <paramref name="quaternion"/> to <see cref="Vector3{T}"/> with euler angles in degrees.
    /// </summary>
    /// <param name="quaternion"><see cref="Quaternion{T}"/> to convert.</param>
    /// <returns>Euler angles of <paramref name="quaternion"/> in degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> ToEulerDegrees<T>(this Quaternion<T> quaternion) where T : IFloatingPointIeee754<T> {
        Vector3<T> r = quaternion.ToEulerRadians();
        return new Vector3<T>(
            FloatingPointIeee754Helper<T>.ConvertRadiansToDegrees(r.X),
            FloatingPointIeee754Helper<T>.ConvertRadiansToDegrees(r.Y),
            FloatingPointIeee754Helper<T>.ConvertRadiansToDegrees(r.Z)
        );
    }

    /// <summary>
    /// Calculates angle in radians between <paramref name="lhs"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Quaternion{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Quaternion{T}"/>.</param>
    /// <returns>Angle in radians between <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AngleRadians<T>(this Quaternion<T> lhs, Quaternion<T> rhs)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        if (lhs.Equals(rhs))
            return T.Zero;

        return T.Acos(T.Min(T.Abs(lhs.Dot(rhs)), T.One)) * NumberHelper<T>.Value2;
    }

    /// <summary>
    /// Calculates angle in degrees between <paramref name="lhs"/> and <paramref name="rhs"/>.
    /// </summary>
    /// <param name="lhs">First <see cref="Quaternion{T}"/>.</param>
    /// <param name="rhs">Second <see cref="Quaternion{T}"/>.</param>
    /// <returns>Angle in degrees between <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AngleDegrees<T>(this Quaternion<T> lhs, Quaternion<T> rhs) where T : IFloatingPointIeee754<T> {
        if (lhs.Equals(rhs))
            return T.Zero;

        return FloatingPointIeee754Helper<T>.ConvertRadiansToDegrees(T.Acos(T.Min(T.Abs(lhs.Dot(rhs)), T.One)));
    }

    /// <summary>
    /// Returns normalized <paramref name="quaternion"/>.
    /// </summary>
    /// <param name="quaternion"><see cref="Quaternion{T}"/> to normalize.</param>
    /// <returns>Normalized <paramref name="quaternion"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> Normalize<T>(this Quaternion<T> quaternion) where T : IFloatingPointIeee754<T> {
        T magnitude = T.Sqrt(quaternion.Dot(quaternion));
        if (magnitude < T.Epsilon)
            return Quaternion<T>.Identity;

        return new Quaternion<T>(
            quaternion.X / magnitude,
            quaternion.Y / magnitude,
            quaternion.Z / magnitude,
            quaternion.W / magnitude
        );
    }

    /// <summary>
    /// Linearly interpolates between <paramref name="lhs"/> and <paramref name="rhs"/> by <paramref name="t"/>.
    /// </summary>
    /// <typeparam name="T">Numeric type used in <see cref="Quaternion{T}"/>.</typeparam>
    /// <param name="lhs">Start <see cref="Quaternion{T}"/>, returned when <paramref name="t"/> = 0.</param>
    /// <param name="rhs">End <see cref="Quaternion{T}"/>, returned when <paramref name="t"/> = 1.</param>
    /// <param name="t">
    /// Not clamped value used to interpolate between <paramref name="lhs"/> and <paramref name="rhs"/>.
    /// </param>
    /// <returns>The interpolated <see cref="Quaternion{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> Lerp<T>(
        this Quaternion<T> lhs, Quaternion<T> rhs, T t
    ) where T : IFloatingPointIeee754<T> {
        T t1 = T.One - t;
        T dot = lhs.Dot(rhs);

        Quaternion<T> result;
        if (dot >= T.Zero) {
            result = new Quaternion<T>(
                t1 * lhs.X + t * rhs.X,
                t1 * lhs.Y + t * rhs.Y,
                t1 * lhs.Z + t * rhs.Z,
                t1 * lhs.W + t * rhs.W
            );
        } else {
            result = new Quaternion<T>(
                t1 * lhs.X - t * rhs.X,
                t1 * lhs.Y - t * rhs.Y,
                t1 * lhs.Z - t * rhs.Z,
                t1 * lhs.W - t * rhs.W
            );
        }

        return result.Normalize();
    }

    /// <summary>
    /// Converts <see cref="Quaternion{T}"/> to quaternion where T is <see cref="float"/>.
    /// </summary>
    /// <param name="quaternion"><see cref="Quaternion{T}"/> to convert.</param>
    /// <returns><see cref="Quaternion{T}"/> with <see cref="float"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<float> ToFloat<T>(this Quaternion<T> quaternion) where T : IConvertible, INumber<T> {
        return new Quaternion<float>(
            VectorHelper.ToFloat(quaternion.X), VectorHelper.ToFloat(quaternion.Y),
            VectorHelper.ToFloat(quaternion.Z), VectorHelper.ToFloat(quaternion.W)
        );
    }

    /// <summary>
    /// Converts <see cref="Quaternion{T}"/> to quaternion where T is <see cref="double"/>.
    /// </summary>
    /// <param name="quaternion"><see cref="Quaternion{T}"/> to convert.</param>
    /// <returns><see cref="Quaternion{T}"/> with <see cref="double"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<double> ToDouble<T>(this Quaternion<T> quaternion) where T : IConvertible, INumber<T> {
        return new Quaternion<double>(
            VectorHelper.ToDouble(quaternion.X), VectorHelper.ToDouble(quaternion.Y),
            VectorHelper.ToDouble(quaternion.Z), VectorHelper.ToDouble(quaternion.W)
        );
    }

    /// <summary>
    /// Converts <see cref="Quaternion{T}"/> to quaternion where T is <see cref="Position"/>.
    /// </summary>
    /// <param name="quaternion"><see cref="Quaternion{T}"/> to convert.</param>
    /// <returns><see cref="Quaternion{T}"/> with <see cref="Position"/> components.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<pos> ToPos<T>(this Quaternion<T> quaternion) where T : IConvertible, INumber<T> {
        return new Quaternion<pos>(
            VectorHelper.ToPos(quaternion.X), VectorHelper.ToPos(quaternion.Y), VectorHelper.ToPos(quaternion.Z),
            VectorHelper.ToPos(quaternion.W)
        );
    }

}
