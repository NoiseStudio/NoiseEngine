using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

[AttributeUsage(System.AttributeTargets.Field)]
internal class OpCodeValidationOptionalAttribute : Attribute {

    public IReadOnlyList<Type> Tail { get; }

    public OpCodeValidationOptionalAttribute(params Type[] tail) {
        Tail = tail;
    }

}
