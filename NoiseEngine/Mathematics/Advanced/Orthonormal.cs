using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics.Advanced;

internal static class Orthonormal {

    internal static void SubspaceBasis<T>(
        Vector3<T> family, Func<Vector3<T>, bool> f
    ) where T : INumber<T>, IFloatingPointIeee754<T> {
        Vector3<T> a;
        if (T.Abs(family.X) > T.Abs(family.Y))
            a = new Vector3<T>(family.Z, T.Zero, -family.X);
        else
            a = new Vector3<T>(T.Zero, -family.Z, family.Y);

        a = a.Normalize();
        if (f(a.Cross(family)))
            f(a);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    internal static Vector3<T> SubspaceBasisOnce<T>(Vector3<T> family) where T : INumber<T>, IFloatingPointIeee754<T> {
        Vector3<T> a;
        if (T.Abs(family.X) > T.Abs(family.Y))
            a = new Vector3<T>(family.Z, T.Zero, -family.X);
        else
            a = new Vector3<T>(T.Zero, -family.Z, family.Y);

        a = a.Normalize();
        return a.Cross(family);
    }

}
