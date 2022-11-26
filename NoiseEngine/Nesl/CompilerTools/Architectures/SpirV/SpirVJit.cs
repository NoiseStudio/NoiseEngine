using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVJit {

    public SpirVCompiler Compiler { get; }

    public SpirVJit(SpirVCompiler compiler) {
        Compiler = compiler;
    }

    public void CompileCode(IEnumerable<Instruction> instructions, NeslMethod neslMethod, SpirVGenerator generator) {
        List<(NeslType type, uint id)> variables = new List<(NeslType type, uint id)>();

        foreach (Instruction instruction in instructions) {
            switch (instruction.OpCode) {

                case OpCode.DefVariable:
                    variables.Add((neslMethod.Assembly.GetType(instruction.ReadUInt64()), instruction.ReadUInt32()));
                    break;
                case OpCode.LoadFloat32:
                    uint index = instruction.ReadUInt32();
                    if (index < neslMethod.Type.Fields.Count) {
                        generator.Emit(
                            SpirVOpCode.OpStore,
                            Compiler.GetSpirVVariable(neslMethod.Type.Fields[(int)index]).Id,
                            Compiler.GetConst(instruction.ReadFloat32())
                        );
                    } else {
                        throw new NotImplementedException();
                    }
                    break;

                case OpCode.Return:
                    generator.Emit(SpirVOpCode.OpReturn);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

}
