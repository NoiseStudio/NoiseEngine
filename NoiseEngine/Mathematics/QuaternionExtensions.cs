using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class QuaternionExtensions {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> ToEulerRadians<T>(this Quaternion<T> quaternion)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        T t0 = (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z) * NumberHelper<T>.Two;
        T t1 = T.One - (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y) * NumberHelper<T>.Two;

        T t2 = (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X) * NumberHelper<T>.Two;
        t2 = T.Min(T.One, t2);
        t2 = t2 < -T.One ? T.One : t2;

        T t3 = (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y) * NumberHelper<T>.Two;
        T t4 = T.One - (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z) * NumberHelper<T>.Two;

        return new Vector3<T>(T.Atan2(t0, t1), T.Asin(t2), T.Atan2(t3, t4));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> ToEulerDegress<T>(this Quaternion<T> quaternion) where T : IFloatingPointIeee754<T> {
        Vector3<T> r = quaternion.ToEulerRadians();
        return new Vector3<T>(
            FloatingPointIeee754Helper.ConvertRadiansToDegress(r.X),
            FloatingPointIeee754Helper.ConvertRadiansToDegress(r.Y),
            FloatingPointIeee754Helper.ConvertRadiansToDegress(r.Z)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AngleRadians<T>(this Quaternion<T> lhs, Quaternion<T> rhs)
        where T : INumber<T>, ITrigonometricFunctions<T>
    {
        if (lhs.Equals(rhs))
            return T.Zero;

        return T.Acos(T.Min(T.Abs(lhs.Dot(rhs)), T.One));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T AngleDegrees<T>(this Quaternion<T> lhs, Quaternion<T> rhs) where T : IFloatingPointIeee754<T> {
        if (lhs.Equals(rhs))
            return T.Zero;

        return FloatingPointIeee754Helper.ConvertRadiansToDegress(T.Acos(T.Min(T.Abs(lhs.Dot(rhs)), T.One)));
    }

    /// <summary>
    /// Returns normalized this <paramref name="quaternion"/>.
    /// </summary>
    /// <param name="quaternion">This <see cref="Quaternion{T}"/>.</param>
    /// <returns>Normalized <paramref name="quaternion"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion<T> Normalize<T>(this Quaternion<T> quaternion) where T : IFloatingPointIeee754<T> {
        T magnitude = T.Sqrt(quaternion.Dot(quaternion));
        if (magnitude > T.Epsilon)
            return Quaternion<T>.Identity;

        return new Quaternion<T>(
            quaternion.X / magnitude,
            quaternion.Y / magnitude,
            quaternion.Z / magnitude,
            quaternion.W / magnitude
        );
    }

}
