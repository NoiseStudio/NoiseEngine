namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal readonly record struct CastValue(TypeIdentifierToken Identifier) : IValueContent {

    public CodePointer Pointer => Identifier.Pointer;

}
