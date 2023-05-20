using NoiseEngine.Nesl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Tests.Environments;

public abstract class NeslParsingTestEnvironment {

    public NeslAssembly CompileSingle(string code) {
        return NeslCompiler.Compile(
            nameof(CompileSingle), "/Abc/", new NeslFile[] { new NeslFile("/Abc/Example", code) }
        );
    }

    public void CompileSingleThrow(string code, params CompilationErrorType[] errors) {
        NeslCompiler.TryCompile(
            nameof(CompileSingleThrow), "/Abc", new NeslFile[] { new NeslFile("/Abc/Example", code) }, null, out _,
            out IEnumerable<CompilationError> output
        );

        if (!output.Any())
            throw new Exception("Expected errors but got nothing.");

        errors = errors.OrderBy(x => (uint)x).ToArray();
        int i = 0;
        foreach (CompilationErrorType error in output.Select(x => x.Type).OrderBy(x => (uint)x)) {
            if (i >= errors.Length)
                throw new Exception($"Expected nothing but got {error}.");
            if (errors[i++] != error)
                throw new Exception($"Expected {errors[i - 1]} but got {error}.");
        }

        if (i < errors.Length)
            throw new Exception($"Expected {errors[i]} but got nothing.");
    }

    public void CompileSingleThrowAny(string code) {
        Assert.False(NeslCompiler.TryCompile(
            nameof(CompileSingleThrow), "/Abc", new NeslFile[] { new NeslFile("/Abc/Example", code) }, null, out _,
            out IEnumerable<CompilationError> output
        ));
    }

}
