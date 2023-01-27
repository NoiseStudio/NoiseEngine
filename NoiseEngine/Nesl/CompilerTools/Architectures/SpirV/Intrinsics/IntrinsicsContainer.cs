using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Intrinsics;

internal abstract class IntrinsicsContainer {

    public SpirVCompiler Compiler => Function.Compiler;
    public NeslMethod NeslMethod => Function.NeslMethod;
    public SpirVGenerator Generator { get; }
    public SpirVFunction Function { get; }
    public IReadOnlyList<SpirVVariable> Parameters { get; }

    protected IntrinsicsContainer(
        SpirVGenerator generator, SpirVFunction function, IReadOnlyList<SpirVVariable> parameters
    ) {
        Generator = generator;
        Function = function;
        Parameters = parameters;
    }

    public abstract void Process();

    protected Exception NewUnableFindDefinitionException() {
        return new InvalidOperationException($"Unable to find given {nameof(IntrinsicAttribute)} definition.");
    }

}
