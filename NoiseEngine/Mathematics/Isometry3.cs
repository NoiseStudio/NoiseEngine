using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Mathematics;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct Isometry3<T>(Vector3<T> Translation, Quaternion<T> Rotation) where T : INumber<T> {

    public Isometry3(Vector3<T> translation) : this(translation, Quaternion<T>.Identity) {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Isometry3<T> InverseMultiplication(Isometry3<T> rhs) {
        Quaternion<T> inverseRotation = Rotation.Inverse();
        Vector3<T> tr12 = rhs.Translation - Translation;
        return new Isometry3<T>(
            inverseRotation * tr12,
            inverseRotation * rhs.Rotation
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3<T> InverseMultiplication(Vector3<T> rhs) {
        return Rotation.Inverse() * (rhs - Translation);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator *(Isometry3<T> isometry, Vector3<T> point) {
        return isometry.Rotation * point + isometry.Translation;
    }

}
