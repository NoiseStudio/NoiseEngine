using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal static class ShaderSettingsExtensions {

    public static uint ToSpirV(this ShaderRoundingMode roundingMode) {
        return roundingMode switch {
            ShaderRoundingMode.ToEven => 0,
            ShaderRoundingMode.ToZero => 1,
            ShaderRoundingMode.ToPositiveInfinity => 2,
            ShaderRoundingMode.ToNegativeInfinity => 3,
            _ => throw new InvalidOperationException("Given rounding mode is not supported.")
        };;
    }

}
