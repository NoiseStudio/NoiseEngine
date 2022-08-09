using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVVariable {

    public SpirVCompiler Compiler { get; }
    public NeslField NeslField { get; }

    public SpirVId Id { get; }

    public SpirVVariable(SpirVCompiler compiler, NeslField neslField) {
        Compiler = compiler;
        NeslField = neslField;

        StorageClass storageClass;
        if (neslField.Attributes.HasAnyAttribute("InAttribute"))
            storageClass = StorageClass.Input;
        else if (neslField.Attributes.HasAnyAttribute("OutAttribute"))
            storageClass = StorageClass.Output;
        else
            storageClass = StorageClass.Private;

        SpirVType type = Compiler.BuiltInTypes.GetOpTypePointer(
            storageClass, Compiler.GetSpirVType(neslField.FieldType));

        Id = Compiler.GetNextId();
        lock (Compiler.TypesAndVariables)
            Compiler.TypesAndVariables.Emit(SpirVOpCode.OpVariable, type.Id, Id, (uint)storageClass);
    }

}
