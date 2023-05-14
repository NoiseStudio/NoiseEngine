using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

internal abstract class ParserExpressionContainer {

    public Parser Parser { get; }
    public NeslAssemblyBuilder Assembly => Parser.Assembly;

    protected ParserExpressionContainer(Parser parser) {
        Parser = parser;
    }
    
}
