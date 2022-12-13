using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.IlCompilation;

internal class DefOperations : IlCompilerOperation {

    private readonly List<SpirVVariable> variables = new List<SpirVVariable>();

    public IReadOnlyList<SpirVVariable> Variables => variables;

    public DefOperations(IlCompiler ilCompiler) : base(ilCompiler) {
    }

    public void DefVariable(Instruction instruction) {
        variables.Add(new SpirVVariable(
            Compiler, Assembly.GetType(instruction.ReadUInt64()), StorageClass.Function, Generator
        ));
    }

}
