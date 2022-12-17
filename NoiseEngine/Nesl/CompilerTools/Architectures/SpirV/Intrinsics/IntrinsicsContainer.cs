using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal abstract class IntrinsicsContainer {

    public SpirVCompiler Compiler { get; }
    public NeslMethod NeslMethod { get; }
    public SpirVGenerator Generator { get; }
    public IReadOnlyList<SpirVVariable> Parameters { get; }

    protected IntrinsicsContainer(
        SpirVCompiler compiler, NeslMethod neslMethod, SpirVGenerator generator,
        IReadOnlyList<SpirVVariable> parameters
    ) {
        Compiler = compiler;
        NeslMethod = neslMethod;
        Generator = generator;
        Parameters = parameters;
    }

    public abstract void Process();

    protected Exception NewUnableFindDefinitionException() {
        return new InvalidOperationException($"Unable to find given {nameof(IntrinsicAttribute)} definition.");
    }

}
