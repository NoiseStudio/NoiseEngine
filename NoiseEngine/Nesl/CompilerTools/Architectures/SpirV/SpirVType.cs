using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;
using System.Reflection;
using System.Reflection.Metadata;

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
        uint index = 0;

        foreach (NeslField field in neslType.Fields) {

            if (field.Name == "Position") {
                lock (Compiler.Annotations) {
                    Compiler.Annotations.Emit(SpirVOpCode.OpDecorate, Id, (uint)Decoration.Block);

                    Compiler.Annotations.Emit(
                        SpirVOpCode.OpMemberDecorate, Id, index.ToSpirVLiteral(), (uint)Decoration.BuiltIn,
                        0u.ToSpirVLiteral()
                    );
                }
            }

            if (!field.IsStatic)
                ids.Add(Compiler.GetSpirVType(field.FieldType).Id);
            index++;
        }

        lock (Compiler.TypesAndVariables)
            Compiler.TypesAndVariables.Emit(SpirVOpCode.OpTypeStruct, Id, ids.AsSpan());
    }

}
