using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVJit {

    public SpirVCompiler Compiler { get; }

    public SpirVJit(SpirVCompiler compiler) {
        Compiler = compiler;
    }

    public void CompileCode(IEnumerable<Instruction> instructions, NeslMethod neslMethod, SpirVGenerator generator) {
        foreach (Instruction instruction in instructions) {
            switch (instruction.OpCode) {

                case OpCode.Return:
                    generator.Emit(SpirVOpCode.OpReturn);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

}
