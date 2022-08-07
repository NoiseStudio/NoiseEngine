using System.Reflection;

namespace NoiseEngine.Nesl.CompilerTools;

internal static class CilCompilerAttributes {

    public static TypeAttributes GetAttributes(NeslType neslType) {
        TypeAttributes result = 0;

        if (neslType.IsClass)
            result |= TypeAttributes.Class;

        return result;
    }

    public static MethodAttributes GetAttributes(NeslMethod neslMethod) {
        MethodAttributes result = 0;

        if (neslMethod.IsStatic)
            result |= MethodAttributes.Static;

        return result;
    }

}
