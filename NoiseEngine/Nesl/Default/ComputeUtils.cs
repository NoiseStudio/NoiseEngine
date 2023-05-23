using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System;

namespace NoiseEngine.Nesl.Default;

internal static class ComputeUtils {

    private const string TypeName = $"System::System.{nameof(ComputeUtils)}";

    public static NeslMethod GlobalInvocation3 =>
        Type.GetMethod(NeslOperators.PropertyGet + nameof(GlobalInvocation3)) ?? throw new NullReferenceException();

    private static NeslType Type => Manager.Assembly.GetType(TypeName) ?? throw new NullReferenceException();

}
