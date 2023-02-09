using System;

namespace NoiseEngine.Nesl.CompilerTools.Parsing.Expressions;

[AttributeUsage(System.AttributeTargets.Field)]
internal class CompilationErrorTypeAttribute : Attribute {

    public CompilationErrorSeverity Severity { get; }

    public CompilationErrorTypeAttribute(CompilationErrorSeverity severity) {
        Severity = severity;
    }

}
