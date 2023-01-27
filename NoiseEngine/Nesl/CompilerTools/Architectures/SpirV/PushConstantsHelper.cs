using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Default;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class PushConstantsHelper {

    public SpirVCompiler Compiler { get; }

    public PushConstantsHelper(SpirVCompiler complier) {
        Compiler = complier;
    }

    public SpirVVariable GetPushConstant(NeslType neslType) {
        SpirVType content = Compiler.GetSpirVStruct(new SpirVType[] { Compiler.GetSpirVType(neslType) });
        SpirVType pointer = Compiler.BuiltInTypes.GetOpTypePointer(StorageClass.PushConstant, content);

        SpirVVariable variable = new SpirVVariable(
            Compiler, neslType, StorageClass.PushConstant, Compiler.GetNextId(), pointer
        );
        Compiler.AddVariable(variable);

        lock (Compiler.TypesAndVariables) {
            Compiler.TypesAndVariables.Emit(
                SpirVOpCode.OpVariable, pointer.Id, variable.Id, (uint)StorageClass.PushConstant
            );
        }

        lock (Compiler.Annotations) {
            Compiler.Annotations.Emit(
                SpirVOpCode.OpMemberDecorate, content.Id, 0u.ToSpirVLiteral(), (uint)Decoration.ColMajor
            );
            Compiler.Annotations.Emit(
                SpirVOpCode.OpMemberDecorate, content.Id, 0u.ToSpirVLiteral(), (uint)Decoration.Offset,
                0u.ToSpirVLiteral()
            );
            Compiler.Annotations.Emit(
                SpirVOpCode.OpMemberDecorate, content.Id, 0u.ToSpirVLiteral(), (uint)Decoration.MatrixStride,
                16u.ToSpirVLiteral()
            );
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, content.Id, (uint)Decoration.Block);
        }

        return variable;
    }

}
