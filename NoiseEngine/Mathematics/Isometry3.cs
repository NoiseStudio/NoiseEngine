using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Mathematics;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct Isometry3<T>(Vector3<T> Translation, Quaternion<T> Rotation) where T : INumber<T> {

    public Isometry3(Vector3<T> translation) : this(translation, Quaternion<T>.Identity) {
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Isometry3<T> ConjugateMultiplication(Isometry3<T> rhs) {
        Quaternion<T> conjugateRotation = Rotation.Conjugate();
        Vector3<T> tr12 = rhs.Translation - Translation;
        return new Isometry3<T>(
            conjugateRotation * tr12,
            conjugateRotation * rhs.Rotation
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3<T> operator *(Isometry3<T> isometry, Vector3<T> point) {
        return isometry.Rotation * point + isometry.Translation;
    }

}
