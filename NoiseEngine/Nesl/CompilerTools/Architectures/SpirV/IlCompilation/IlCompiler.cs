using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class IlCompiler {

    private readonly IEnumerable<Instruction> instructions;

    public SpirVCompiler Compiler { get; }
    public NeslMethod NeslMethod { get; }
    public SpirVGenerator Generator { get; }
    public IReadOnlyList<SpirVVariable> Parameters { get; }
    public SpirVFunction Function { get; }
    public ExecutionModel? ExecutionModel { get; }
    public HashSet<SpirVVariable> UsedVariables { get; } = new HashSet<SpirVVariable>();

    public ArithmeticOperations ArithmeticOperations { get; }
    public BranchOperations BranchOperations { get; }
    public DefOperations DefOperations { get; }
    public LoadOperations LoadOperations { get; }
    public LoadElementOperations LoadElementOperations { get; }
    public LoadFieldOperations LoadFieldOperations { get; }

    public IlCompiler(
        SpirVCompiler compiler, IEnumerable<Instruction> instructions, NeslMethod neslMethod, SpirVGenerator generator,
        IReadOnlyList<SpirVVariable> parameters, SpirVFunction function, ExecutionModel? executionModel
    ) {
        Compiler = compiler;
        this.instructions = instructions;
        NeslMethod = neslMethod;
        Generator = generator;
        Function = function;
        Parameters = parameters;
        ExecutionModel = executionModel;

        ArithmeticOperations = new ArithmeticOperations(this);
        BranchOperations = new BranchOperations(this);
        DefOperations = new DefOperations(this);
        LoadOperations = new LoadOperations(this);
        LoadElementOperations = new LoadElementOperations(this);
        LoadFieldOperations = new LoadFieldOperations(this);
    }

    public void Compile() {
        Instruction lastInstruction = default;
        foreach (Instruction instruction in instructions) {
            lastInstruction = instruction;
            switch (instruction.OpCode) {
                #region ArithmeticOperations

                case OpCode.Negate:
                    ArithmeticOperations.Negate(instruction);
                    break;
                case OpCode.Add:
                    ArithmeticOperations.Add(instruction);
                    break;
                case OpCode.Subtract:
                    ArithmeticOperations.Subtract(instruction);
                    break;
                case OpCode.Multiple:
                    ArithmeticOperations.Multiple(instruction);
                    break;
                case OpCode.Divide:
                    ArithmeticOperations.Divide(instruction);
                    break;
                case OpCode.Modulo:
                    ArithmeticOperations.Modulo(instruction);
                    break;
                case OpCode.Remainder:
                    ArithmeticOperations.Remainder(instruction);
                    break;
                case OpCode.Power:
                    ArithmeticOperations.Power(instruction);
                    break;

                #endregion
                #region BranchOperations

                case OpCode.Call:
                    BranchOperations.Call(instruction);
                    break;
                case OpCode.Return:
                    BranchOperations.Return();
                    break;
                case OpCode.ReturnValue:
                    BranchOperations.ReturnValue(instruction);
                    break;

                #endregion
                #region DefOperations

                case OpCode.DefVariable:
                    DefOperations.DefVariable(instruction);
                    break;

                #endregion
                #region LoadOperations

                case OpCode.Load:
                    LoadOperations.Load(instruction);
                    break;
                case OpCode.LoadUInt32:
                    LoadOperations.LoadUInt32(instruction);
                    break;
                case OpCode.LoadFloat32:
                    LoadOperations.LoadFloat32(instruction);
                    break;

                #endregion
                #region LoadElementOperations

                case OpCode.LoadElement:
                    LoadElementOperations.LoadElement(instruction);
                    break;
                case OpCode.SetElement:
                    LoadElementOperations.SetElement(instruction);
                    break;

                #endregion
                #region LoadFieldOperations

                case OpCode.LoadField:
                    LoadFieldOperations.LoadField(instruction);
                    break;
                case OpCode.SetField:
                    LoadFieldOperations.SetField(instruction);
                    break;

                #endregion
                default:
                    throw new NotImplementedException();
            }
        }

        if (NeslMethod.ReturnType is null) {
            if (lastInstruction.OpCode != OpCode.Return)
                throw new InvalidOperationException($"{NeslMethod} does not end with a return opcode.");
        } else if (lastInstruction.OpCode != OpCode.ReturnValue) {
            throw new InvalidOperationException($"{NeslMethod} does not end with a return value opcode.");
        }
    }

}
