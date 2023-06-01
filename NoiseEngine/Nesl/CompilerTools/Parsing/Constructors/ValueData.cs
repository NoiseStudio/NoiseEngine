using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using System;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal readonly record struct ValueData(NeslType? Type, uint Id, object? AdditionalData) {

    public static ValueData Invalid => new ValueData(null, uint.MaxValue, null);

    public bool IsInvalid => this == Invalid;

    public ValueData(NeslType type, uint id) : this(type, id, null) {
    }

    public bool CheckLoadConst(NeslType expected) {
        const string Start = "System::System.";

        if (AdditionalData is not ConstValueToken constValue)
            return Type == expected;

        return expected.FullNameWithAssembly switch {
            Start + "Float32" => constValue.ToFloat32(out _, out _),
            Start + "UInt32" => constValue.ToUInt32(out _, out _),
            _ => false
        };
    }

    public ValueData LoadConst(Parser parser, NeslType expected) {
        const string Start = "System::System.";

        if (AdditionalData is not ConstValueToken constValue)
            return this;

        IlGenerator il = parser.CurrentMethod.IlGenerator;
        uint id = uint.MaxValue;
        CompilationError error = default;

        switch (expected.FullNameWithAssembly) {
            case Start + "Float32":
                if (constValue.ToFloat32(out float a, out error)) {
                    id = il.GetNextVariableId();
                    il.Emit(OpCode.DefVariable, expected);
                    il.Emit(OpCode.LoadFloat32, id, a);
                }
                break;
            case Start + "UInt32":
                if (constValue.ToUInt32(out uint b, out error)) {
                    id = il.GetNextVariableId();
                    il.Emit(OpCode.DefVariable, expected);
                    il.Emit(OpCode.LoadUInt32, id, b);
                }
                break;
            default:
                parser.Throw(new CompilationError(
                    constValue.Pointer, CompilationErrorType.ImplicitCastOperatorNotFound, constValue
                ));
                break;
        }

        if (id == uint.MaxValue)
            parser.Throw(error);
        return new ValueData(expected, id);
    }

    public NeslType GetBestMatchConstType() {
        if (AdditionalData is not ConstValueToken constValue)
            return Type ?? throw new UnreachableException();

        // TODO: Change to 64 bit types.
        return constValue.Type switch {
            ConstValueType.UnsignedInteger => BuiltInTypes.UInt32,
            ConstValueType.Float => BuiltInTypes.Float32,
            _ => throw new NotImplementedException()
        };
    }

}
