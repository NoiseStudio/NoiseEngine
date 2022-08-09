using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Tests.Nesl;

internal static class TestEmitHelper {

    private static readonly NeslAssemblyBuilder assembly = NewAssembly();
    private static readonly NeslTypeBuilder type = NewType();

    public static NeslAssemblyBuilder NewAssembly() {
        return NeslAssemblyBuilder.DefineAssembly(Guid.NewGuid().ToString());
    }

    public static NeslTypeBuilder NewType() {
        return assembly.DefineType(Guid.NewGuid().ToString());
    }

    public static NeslFieldBuilder NewField() {
        return type.DefineField(Guid.NewGuid().ToString(), BuiltInTypes.Float32);
    }

    public static NeslMethodBuilder NewMethod() {
        return type.DefineMethod(Guid.NewGuid().ToString(), BuiltInTypes.Float32, BuiltInTypes.Float32);
    }

}
