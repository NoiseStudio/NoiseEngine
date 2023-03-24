namespace NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

internal interface IParserToken {

    public bool IsIgnored { get; }
    public int Priority { get; }

}
