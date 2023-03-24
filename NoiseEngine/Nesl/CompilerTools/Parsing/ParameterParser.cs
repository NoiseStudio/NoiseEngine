using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;
using NoiseEngine.Nesl.Emit;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing;

internal class ParameterParser : Parser {

    private List<(NeslType type, string name)>? definedParameters;

    public IReadOnlyList<(NeslType type, string name)> DefinedParameters =>
        definedParameters is null ? Array.Empty<(NeslType, string)>() : definedParameters;

    public ParameterParser(
        Parser? parent, NeslAssemblyBuilder assembly, ParserStep step, TokenBuffer buffer
    ) : base(parent, assembly, step, buffer) {
    }

    public void TryDefineParameter(TypeIdentifierToken typeIdentifier, NameToken name) {
        definedParameters ??= new List<(NeslType type, string name)>();
        foreach ((_, string n) in definedParameters) {
            if (name.Name == n) {
                Throw(new CompilationError(name.Pointer, CompilationErrorType.ParameterAlreadyExists, name.Name));
                return;
            }
        }

        if (TryGetType(typeIdentifier, out NeslType? type))
            definedParameters.Add((type, name.Name));
    }

}
