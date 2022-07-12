using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

public static class Vector3Extensions {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Magnitude<T>(this Vector3<T> vector) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(vector.MagnitudeSquared());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> Normalize<T>(this Vector3<T> vector) where T : INumber<T>, IFloatingPointIeee754<T> {
        T magnitude = vector.Magnitude();
        return magnitude > T.Epsilon ? (vector / magnitude) : Vector3<T>.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Distance<T>(this Vector3<T> lhs, Vector3<T> rhs) where T : INumber<T>, IRootFunctions<T> {
        return T.Sqrt(lhs.DistanceSquared(rhs));
    }

}
