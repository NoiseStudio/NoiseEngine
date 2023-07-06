using NoiseEngine.Nesl.Emit;
using System.Diagnostics;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal static class ConstructorHelper {

    public static void AppendHeader(NeslMethodBuilder method) {
        NeslType type = method.ReturnType ?? throw new UnreachableException();
        IlGenerator il = method.IlGenerator;

        uint a = il.GetNextVariableId();
        Debug.Assert(type.Fields.Count + method.ParameterTypes.Count == a);

        il.Emit(OpCode.DefVariable, type);
    }

    public static void AppendFooter(NeslMethodBuilder method) {
        NeslType type = method.ReturnType ?? throw new UnreachableException();
        IlGenerator il = method.IlGenerator;

        il.Emit(OpCode.ReturnValue, (uint)type.Fields.Count + (uint)method.ParameterTypes.Count);
    }

}
