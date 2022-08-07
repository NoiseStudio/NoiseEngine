using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

[AttributeUsage(System.AttributeTargets.Field)]
internal class OpCodeValidationAttribute : Attribute {

    public IReadOnlyList<Type> Tail { get; }

    public OpCodeValidationAttribute(params Type[] tail) {
        Tail = tail;
    }

}
