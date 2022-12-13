using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class IlCompiler {

    private readonly IEnumerable<Instruction> instructions;

    public SpirVCompiler Compiler { get; }
    public NeslMethod NeslMethod { get; }
    public SpirVGenerator Generator { get; }

    public DefOperations DefOperations { get; }
    public LoadOperations LoadOperations { get; }

    public IlCompiler(
        SpirVCompiler compiler, IEnumerable<Instruction> instructions, NeslMethod neslMethod, SpirVGenerator generator
    ) {
        Compiler = compiler;
        this.instructions = instructions;
        NeslMethod = neslMethod;
        Generator = generator;

        DefOperations = new DefOperations(this);
        LoadOperations = new LoadOperations(this);
    }

    public void Compile() {
        foreach (Instruction instruction in instructions) {
            switch (instruction.OpCode) {

                case OpCode.DefVariable:
                    DefOperations.DefVariable(instruction);
                    break;

                case OpCode.Load:
                    LoadOperations.Load(instruction);
                    break;
                case OpCode.LoadFloat32:
                    LoadOperations.LoadFloat32(instruction);
                    break;

                case OpCode.Return:
                    Generator.Emit(SpirVOpCode.OpReturn);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

}
