using System.Numerics;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Mathematics;

internal static class TrigonometricFunctionsHelper {

    // TODO: After .NET 7 release check if some interfaces have Atan2 implementation.
    // See https://github.com/dotnet/core/issues/7683#issuecomment-1210199763 for more informations.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Atan2<T>(T y, T x) where T : ITrigonometricFunctions<T> {
        return T.Atan(y / x);
    }

}
