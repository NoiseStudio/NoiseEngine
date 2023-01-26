using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;
using System.Collections.Concurrent;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVBuiltInVariables {

    private readonly ConcurrentDictionary<(NeslType, StorageClass, uint), Lazy<SpirVVariable>> builtIns =
        new ConcurrentDictionary<(NeslType, StorageClass, uint), Lazy<SpirVVariable>>();

    public SpirVCompiler Compiler { get; }

    public SpirVBuiltInVariables(SpirVCompiler compiler) {
        Compiler = compiler;
    }

    public SpirVVariable GetBuiltIn(NeslType neslType, StorageClass storageClass, uint type) {
        return builtIns.GetOrAdd((neslType, storageClass, type), _ => new Lazy<SpirVVariable>(() => {
            SpirVVariable variable = new SpirVVariable(Compiler, neslType, storageClass, Compiler.TypesAndVariables);
            Compiler.AddVariable(variable);

            Compiler.Annotations.Emit(
                SpirVOpCode.OpDecorate, variable.Id, (uint)Decoration.BuiltIn, type.ToSpirVLiteral()
            );

            return variable;
        })).Value;
    }

}
