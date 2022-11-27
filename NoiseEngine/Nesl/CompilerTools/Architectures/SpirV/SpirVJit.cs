using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
                        NeslField field = neslMethod.Type.Fields[(int)index];
                        SpirVVariable variable = Compiler.GetSpirVVariable(field);
                        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(
                            StorageClass.Uniform, Compiler.GetSpirVType(field.FieldType)
                        );

                        SpirVId id = Compiler.GetNextId();
                        generator.Emit(
                            SpirVOpCode.OpAccessChain, pointer.Id, id, variable.Id,
                            new SpirVId[] { Compiler.GetConst(0) }
                        );

                        generator.Emit(
                            SpirVOpCode.OpStore,
                            id,
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
