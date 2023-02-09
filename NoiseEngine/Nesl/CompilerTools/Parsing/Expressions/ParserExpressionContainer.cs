namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal abstract class ParserExpressionContainer {

    public Parser Parser { get; }

    protected ParserExpressionContainer(Parser parser) {
        Parser = parser;
    }
    
}
