using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVVariable {

    public SpirVCompiler Compiler { get; }
    public NeslType NeslType { get; }
    public StorageClass StorageClass { get; }

    public SpirVId Id { get; }
    public uint Binding { get; }

    public SpirVVariable(SpirVCompiler compiler, NeslType neslType, StorageClass storageClass) {
        Compiler = compiler;
        NeslType = neslType;
        StorageClass = storageClass;

        SpirVType type = Compiler.GetSpirVType(neslType);
        SpirVType structure = Compiler.GetSpirVStruct(new SpirVType[] { type });
        SpirVType structurePointer = Compiler.BuiltInTypes.GetOpTypePointer(storageClass, structure);

        Id = Compiler.GetNextId();
        lock (Compiler.TypesAndVariables)
            Compiler.TypesAndVariables.Emit(SpirVOpCode.OpVariable, structurePointer.Id, Id, (uint)storageClass);

        lock (Compiler.Annotations) {
            Compiler.Annotations.Emit(
                SpirVOpCode.OpMemberDecorate, structure.Id, 0u.ToSpirVLiteral(), (uint)Decoration.Offset,
                0u.ToSpirVLiteral()
            );
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, structure.Id, (uint)Decoration.BufferBlock);
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, Id, (uint)Decoration.DescriptorSet, 0u.ToSpirVLiteral());

            Binding = Compiler.GetDescriptorSet(0u).NextBinding();
            Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, Id, (uint)Decoration.Binding, Binding.ToSpirVLiteral());
        }
    }

    public SpirVVariable(SpirVCompiler compiler, NeslField neslField) :
        this(compiler, neslField.FieldType, GetStorageClass(neslField)) {
    }

    private static StorageClass GetStorageClass(NeslField neslField) {
        if (neslField.Attributes.HasAnyAttribute("InAttribute"))
            return StorageClass.Input;
        if (neslField.Attributes.HasAnyAttribute("OutAttribute"))
            return StorageClass.Output;
        if (neslField.Attributes.HasAnyAttribute(nameof(UniformAttribute)))
            return StorageClass.Uniform;
        return StorageClass.Private;
    }

}
