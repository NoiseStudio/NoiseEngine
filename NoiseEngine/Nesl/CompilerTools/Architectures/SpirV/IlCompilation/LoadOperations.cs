using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class LoadOperations : IlCompilerOperation {

    public LoadOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public static SpirVId SpirVLoad(SpirVGenerator generator, SpirVVariable variable) {
        SpirVCompiler compiler = variable.Compiler;
        SpirVId id = compiler.GetNextId();
        generator.Emit(
            SpirVOpCode.OpLoad, compiler.GetSpirVType(variable.NeslType).Id, id, variable.GetAccess(generator)
        );
        return id;
    }

    public static SpirVId GetAccessChainFromIndex(
        SpirVGenerator generator, SpirVType destinationType, SpirVVariable sourceVariable, SpirVId index
    ) {
        SpirVCompiler compiler = sourceVariable.Compiler;
        SpirVType pointer = compiler.BuiltInTypes.GetOpTypePointer(sourceVariable.StorageClass, destinationType);

        bool isSurrounded = sourceVariable.StorageClass == StorageClass.Uniform;
        Span<SpirVId> indexes = stackalloc SpirVId[isSurrounded ? 2 : 1];

        if (isSurrounded) {
            indexes[0] = compiler.GetConst(0);
            indexes[1] = index;
        } else {
            indexes[0] = index;
        }

        SpirVId id = compiler.GetNextId();
        generator.Emit(
            SpirVOpCode.OpAccessChain, pointer.Id, id, sourceVariable.Id, indexes
        );

        return id;
    }

    public SpirVId SpirVLoad(SpirVVariable variable) {
        return SpirVLoad(Generator, variable);
    }

    public SpirVId GetAccessChainFromIndex(SpirVType destinationType, SpirVVariable sourceVariable, SpirVId index) {
        return GetAccessChainFromIndex(Generator, destinationType, sourceVariable, index);
    }

    public void SpirVStore(SpirVVariable variable, SpirVId constId) {
        Generator.Emit(SpirVOpCode.OpStore, variable.GetAccess(Generator), constId);
    }

    public void Load(Instruction instruction) {
        SpirVVariable result = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;
        SpirVVariable value = instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!;

        if (value.AdditionalData is SpirVVariable[] innerVariables) {
            for (uint i = 0; i < innerVariables.Length; i++) {
                SpirVVariable innerVariable = innerVariables[i];
                SpirVId load = SpirVLoad(innerVariable);
                SpirVId accessChain = GetAccessChainFromIndex(
                    Compiler.GetSpirVType(innerVariable.NeslType), result, Compiler.GetConst(i)
                );

                Generator.Emit(SpirVOpCode.OpStore, accessChain, load);
            }
            return;
        }

        SpirVId id = Compiler.GetNextId();
        Generator.Emit(SpirVOpCode.OpLoad, Compiler.GetSpirVType(result.NeslType).Id, id, value.GetAccess(Generator));
        SpirVStore(result, id);
    }

    public void LoadUInt32(Instruction instruction) {
        SpirVStore(instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!, Compiler.GetConst(instruction.ReadUInt32()));
    }

    public void LoadFloat32(Instruction instruction) {
        SpirVStore(instruction.ReadSpirVVariable(IlCompiler, NeslMethod)!, Compiler.GetConst(instruction.ReadFloat32()));
    }

}
