using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.Cil;

internal class CilJit {

    public CilCompiler Compiler { get; }

    public CilJit(CilCompiler compiler) {
        Compiler = compiler;
    }

    public void CompileCode(IEnumerable<Instruction> instructions, NeslMethod neslMethod, ILGenerator generator) {
        foreach (Instruction instruction in instructions) {
            switch (instruction.OpCode) {

                case Emit.OpCode.LoadArg:
                    generator.Emit(OpCodes.Ldarg, (int)instruction.ReadUInt8());
                    break;
                case Emit.OpCode.LoadField:
                    generator.Emit(OpCodes.Ldfld,
                        Compiler.GetCilField(neslMethod.Type.GetField(instruction.ReadUInt32())));
                    break;
                case Emit.OpCode.LoadUInt32:
                    generator.Emit(OpCodes.Ldc_I4, instruction.ReadUInt32());
                    break;
                case Emit.OpCode.LoadUInt64:
                    generator.Emit(OpCodes.Ldc_I8, instruction.ReadUInt64());
                    break;
                case Emit.OpCode.LoadFloat32:
                    generator.Emit(OpCodes.Ldc_R4, instruction.ReadFloat32());
                    break;

                case Emit.OpCode.SetElement:
                    generator.Emit(OpCodes.Stelem,
                        Compiler.GetCilTypeOutput(neslMethod.Assembly.GetType(instruction.ReadUInt64())));
                    break;

                case Emit.OpCode.Add:
                    generator.Emit(OpCodes.Add);
                    break;
                case Emit.OpCode.Sub:
                    generator.Emit(OpCodes.Sub);
                    break;
                case Emit.OpCode.Mul:
                    generator.Emit(OpCodes.Mul);
                    break;
                case Emit.OpCode.Div:
                    generator.Emit(OpCodes.Div);
                    break;

                case Emit.OpCode.Call:
                    generator.Emit(OpCodes.Call,
                        Compiler.GetCilMethod(neslMethod.Assembly.GetMethod(instruction.ReadUInt64())));
                    break;
                case Emit.OpCode.Return:
                    generator.Emit(OpCodes.Ret);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }

}
