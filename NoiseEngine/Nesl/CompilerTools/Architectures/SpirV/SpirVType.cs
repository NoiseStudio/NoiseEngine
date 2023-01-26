using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVType {

    public SpirVCompiler Compiler { get; }
    public NeslType? NeslType { get; }

    public SpirVId Id { get; }

    public SpirVType(SpirVCompiler compiler, SpirVId id) {
        Compiler = compiler;
        Id = id;
    }

    public SpirVType(SpirVCompiler compiler, NeslType neslType) {
        Compiler = compiler;
        NeslType = neslType;

        Id = Compiler.GetNextId();

        FastList<SpirVId> ids = new FastList<SpirVId>();
        foreach (NeslField field in neslType.Fields) {
            if (!field.IsStatic)
                ids.Add(Compiler.GetSpirVType(field.FieldType).Id);
        }

        lock (Compiler.TypesAndVariables)
            Compiler.TypesAndVariables.Emit(SpirVOpCode.OpTypeStruct, Id, ids.AsSpan());
    }

}
