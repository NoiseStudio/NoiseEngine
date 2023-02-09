using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

[AttributeUsage(System.AttributeTargets.Method)]
internal class ParserExpressionAttribute : Attribute {

    public ParserStep Step { get; }

    public ParserExpressionAttribute(ParserStep step) {
        Step = step;
    }

}
