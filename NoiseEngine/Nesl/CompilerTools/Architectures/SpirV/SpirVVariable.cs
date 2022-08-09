using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVVariable {

    public SpirVCompiler Compiler { get; }
    public NeslType NeslType { get; }
    public StorageClass StorageClass { get; }

    public SpirVId Id { get; }

    public SpirVVariable(SpirVCompiler compiler, NeslType neslType, StorageClass storageClass) {
        Compiler = compiler;
        NeslType = neslType;
        StorageClass = storageClass;

        SpirVType type = Compiler.BuiltInTypes.GetOpTypePointer(storageClass, Compiler.GetSpirVType(neslType));

        Id = Compiler.GetNextId();
        lock (Compiler.TypesAndVariables)
            Compiler.TypesAndVariables.Emit(SpirVOpCode.OpVariable, type.Id, Id, (uint)storageClass);
    }

    public SpirVVariable(SpirVCompiler compiler, NeslField neslField) :
        this(compiler, neslField.ParentType, GetStorageClass(neslField)) {
    }

    private static StorageClass GetStorageClass(NeslField neslField) {
        if (neslField.Attributes.HasAnyAttribute("InAttribute"))
            return StorageClass.Input;
        if (neslField.Attributes.HasAnyAttribute("OutAttribute"))
            return StorageClass.Output;
        return StorageClass.Private;
    }

}
