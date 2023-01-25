using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class ArithmeticOperations : IlCompilerOperation {

    public ArithmeticOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    private static Exception NewNotSupportedType() {
        return new InvalidOperationException("Used NESL type is not supported in this instruction.");
    }

    public void Negate(Instruction instruction) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable value = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        SpirVId resultId = Compiler.GetNextId();
        Generator.Emit(
            OpCodeSelector(result, null, SpirVOpCode.OpSNegate, SpirVOpCode.OpFNegate),
            Compiler.GetSpirVType(result.NeslType).Id, resultId, IlCompiler.LoadOperations.SpirVLoad(value)
        );

        IlCompiler.LoadOperations.SpirVStore(result, resultId);
    }

    public void Add(Instruction instruction) {
        TwoOperandHelper(instruction, SpirVOpCode.OpIAdd, SpirVOpCode.OpIAdd, SpirVOpCode.OpFAdd);
    }

    public void Subtract(Instruction instruction) {
        TwoOperandHelper(instruction, SpirVOpCode.OpISub, SpirVOpCode.OpISub, SpirVOpCode.OpFSub);
    }

    public void Multiple(Instruction instruction) {
        TwoOperandHelper(instruction, SpirVOpCode.OpIMul, SpirVOpCode.OpIMul, SpirVOpCode.OpFMul);
    }

    public void Divide(Instruction instruction) {
        TwoOperandHelper(instruction, SpirVOpCode.OpUDiv, SpirVOpCode.OpSDiv, SpirVOpCode.OpFDiv);
    }

    public void Modulo(Instruction instruction) {
        TwoOperandHelper(instruction, SpirVOpCode.OpUMod, SpirVOpCode.OpSMod, SpirVOpCode.OpFMod);
    }

    public void Remainder(Instruction instruction) {
        TwoOperandHelper(instruction, SpirVOpCode.OpUMod, SpirVOpCode.OpSRem, SpirVOpCode.OpFRem);
    }

    private void TwoOperandHelper(
        Instruction instruction, SpirVOpCode? uintOpCode, SpirVOpCode intOpCode, SpirVOpCode floatOpCode
    ) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable operand1 = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable operand2 = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        SpirVId resultId = Compiler.GetNextId();
        Generator.Emit(
            OpCodeSelector(result, uintOpCode, intOpCode, floatOpCode), Compiler.GetSpirVType(result.NeslType).Id,
            resultId, IlCompiler.LoadOperations.SpirVLoad(operand1), IlCompiler.LoadOperations.SpirVLoad(operand2)
        );

        IlCompiler.LoadOperations.SpirVStore(result, resultId);
    }

    private SpirVOpCode OpCodeSelector(
        SpirVVariable variable, SpirVOpCode? uintOpCode, SpirVOpCode intOpCode, SpirVOpCode floatOpCode
    ) {
        if (variable.NeslType.Assembly.Name != "System")
            throw NewNotSupportedType();

        SpirVOpCode? opCode = OpCodeSelectorWorker(variable.NeslType, uintOpCode, intOpCode, floatOpCode);
        if (opCode is null && OpCodeSelectorWorkerIsVector(variable.NeslType))
            opCode = OpCodeSelectorWorker(variable.NeslType.Fields[0].FieldType, uintOpCode, intOpCode, floatOpCode);

        if (opCode is null)
            throw NewNotSupportedType();

        return opCode.Value;
    }

    private SpirVOpCode? OpCodeSelectorWorker(
        NeslType neslType, SpirVOpCode? uintOpCode, SpirVOpCode intOpCode, SpirVOpCode floatOpCode
    ) {
        return neslType.FullName switch {
            "System.UInt32" => uintOpCode ?? throw NewNotSupportedType(),
            "System.UInt64" => uintOpCode ?? throw NewNotSupportedType(),
            "System.Int32" => intOpCode,
            "System.Int64" => intOpCode,
            "System.Float32" => floatOpCode,
            "System.Float64" => floatOpCode,
            _ => null
        };
    }

    private bool OpCodeSelectorWorkerIsVector(NeslType neslType) {
        return neslType.FullName switch {
            "System.Vector2`1" => true,
            "System.Vector3`1" => true,
            "System.Vector4`1" => true,
            _ => false
        };
    }

}
