using System.Numerics;
using System.Runtime.CompilerServices;

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
            FloatingPointIeee754Helper.ConvertRadiansToDegrees(r.X),
            FloatingPointIeee754Helper.ConvertRadiansToDegrees(r.Y),
            FloatingPointIeee754Helper.ConvertRadiansToDegrees(r.Z)
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

        return FloatingPointIeee754Helper.ConvertRadiansToDegrees(T.Acos(T.Min(T.Abs(lhs.Dot(rhs)), T.One)));
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

}
