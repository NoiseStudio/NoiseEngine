using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

[AttributeUsage(System.AttributeTargets.Field)]
internal class ParserTokenAttribute : Attribute {

    public Type Type { get; }

    public ParserTokenAttribute(Type type) {
        Type = type;
    }

}
