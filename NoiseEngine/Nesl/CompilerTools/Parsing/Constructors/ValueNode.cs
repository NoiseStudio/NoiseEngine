using NoiseEngine.Nesl.CompilerTools.Parsing.Tokens;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal record ValueNode(IValueNodeElement Left, OperatorToken Operator, IValueNodeElement Right) : IValueNodeElement;
