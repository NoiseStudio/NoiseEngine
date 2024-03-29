﻿using NoiseEngine.Nesl.CompilerTools;
using NoiseEngine.Nesl.CompilerTools.Parsing;
using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Nesl;

public static class NeslCompiler {

    /// <summary>
    /// Compiles the given <paramref name="files"/> into a <see cref="NeslAssembly"/>.
    /// </summary>
    /// <param name="assemblyName">Name of new <see cref="NeslAssembly"/>.</param>
    /// <param name="assemblyPath">Path of directory with <paramref name="files"/>.</param>
    /// <param name="files"><see cref="NeslFile"/>s to compile.</param>
    /// <param name="dependencies">
    /// <see cref="NeslAssembly"/>s which is used as dependencies in compiled code. Regardless of the state, it always
    /// contains the default library.
    /// </param>
    /// <returns>New <see cref="NeslAssembly"/>.</returns>
    public static NeslAssembly Compile(
        string assemblyName, string assemblyPath, IEnumerable<NeslFile> files,
        IEnumerable<NeslAssembly>? dependencies = null
    ) {
        bool result = TryCompile(
            assemblyName, assemblyPath, files, dependencies, out NeslAssembly? assembly,
            out IEnumerable<CompilationError> errors
        );

        StringBuilder exceptionBuilder = new StringBuilder("Unable to compile NESL files.");
        exceptionBuilder.AppendLine();

        StringBuilder logBuilder = new StringBuilder("Nesl compilation output.");
        logBuilder.AppendLine();

        int i = 0;
        foreach (CompilationError error in errors) {
            string s = error.ToString();
            if (error.Severity == CompilationErrorSeverity.Error)
                exceptionBuilder.AppendLine(s);
            logBuilder.AppendLine(s);
            i++;
        }

        if (i > 0)
            Log.Warning(logBuilder.ToString());

        if (result)
            return assembly!;

        throw new NeslCompilationException(exceptionBuilder.ToString());
    }

    /// <summary>
    /// Tries compiles the given <paramref name="files"/> into a <see cref="NeslAssembly"/>.
    /// </summary>
    /// <param name="assemblyName">Name of new <see cref="NeslAssembly"/>.</param>
    /// <param name="assemblyPath">Path of directory with <paramref name="files"/>.</param>
    /// <param name="files"><see cref="NeslFile"/>s to compile.</param>
    /// <param name="dependencies">
    /// <see cref="NeslAssembly"/>s which is used as dependencies in compiled code. Regardless of the state, it always
    /// contains the default library.
    /// </param>
    /// <param name="assembly">
    /// New <see cref="NeslAssembly"/> or <see langword="null"/> when result is <see langword="false"/>.
    /// </param>
    /// <param name="errors">Compilation errors, warnings etc.</param>
    /// <returns>
    /// <see langword="true"/> when compilation produces zero errors; otherwise <see langword="false"/>.
    /// </returns>
    public static bool TryCompile(
        string assemblyName, string assemblyPath, IEnumerable<NeslFile> files, IEnumerable<NeslAssembly>? dependencies,
        [NotNullWhen(true)] out NeslAssembly? assembly, out IEnumerable<CompilationError> errors
    ) {
        IEnumerable<NeslAssembly> d = new NeslAssembly[] { Default.Manager.Assembly };
        if (dependencies is not null)
            d = d.Concat(dependencies);
        return TryCompileWithoutDefault(assemblyName, assemblyPath, files, d, out assembly, out errors);
    }

    internal static bool TryCompileWithoutDefault(
        string assemblyName, string assemblyPath, IEnumerable<NeslFile> files, IEnumerable<NeslAssembly> dependencies,
        [NotNullWhen(true)] out NeslAssembly? assembly, out IEnumerable<CompilationError> errors
    ) {
        NeslAssemblyBuilder assemblyBuilder = NeslAssemblyBuilder.DefineAssemblyWithoutDefault(
            assemblyName, dependencies
        );

        NeslFile[] filesArray = files.ToArray();
        Parser[] parsers = new Parser[filesArray.Length];

        int fileIndex = -1;
        ParserStorage storage = new ParserStorage();
        Parallel.For(0, Math.Min(Environment.ProcessorCount, filesArray.Length), _ => {
            Lexer lexer = new Lexer();
            int i;
            while ((i = Interlocked.Increment(ref fileIndex)) < filesArray.Length) {
                NeslFile file = filesArray[i];
                TokenBuffer buffer = new TokenBuffer(lexer.Lex(file.Path, file.Code));
                Parser parser = new Parser(storage, null, assemblyBuilder, assemblyPath, ParserStep.TopLevel, buffer);

                if (!parser.TryDefineUsing(new TypeIdentifierToken(
                    default, parser.GetNamespaceFromFilePath(file.Path), Array.Empty<TypeIdentifierToken>()
                ))) {
                    throw new UnreachableException();
                }

                parser.Parse();
                parsers[i] = parser;
            }
        });

        Parallel.ForEach(parsers, (parser, _) => parser.AnalyzeTypes());
        IEnumerable<Parser> p = parsers.SelectMany(x => x.Types.Append(x));
        Parallel.ForEach(p, (parser, _) => parser.AnalyzeFields());
        Parallel.ForEach(p, (parser, _) => parser.AnalyzeMethods());
        Parallel.ForEach(p, (parser, _) => parser.AnalyzeTypeDependencies());

        storage.CreateGenerics = true;
        if (parsers.Length > 0)
            storage.InitializeGenericMakedTypes(parsers[0]);

        Parallel.ForEach(p, (parser, _) => parser.ConstructType());
        Parallel.ForEach(p, (parser, _) => parser.AnalyzeMethodBodies());
        Parallel.ForEach(parsers, (parser, _) => parser.AnalyzeUsings());

        if (parsers.Length > 0)
            storage.CheckGenericConstraintSatisfying(parsers[0]);

        errors = p.Concat(p.SelectMany(x => x.Methods)).SelectMany(x => x.Errors).OrderBy(x => x.Path)
            .ThenBy(x => x.Line).ThenBy(x => x.Column).ToArray();

        if (errors.Any(x => x.Severity == CompilationErrorSeverity.Error)) {
            assembly = null;
            return false;
        }

        assembly = assemblyBuilder;
        return true;
    }

}
