using System;

namespace NoiseEngine.Nesl.Emit;

[AttributeUsage(System.AttributeTargets.Field)]
internal class OpCodeValidationOptionalAttribute : OpCodeValidationAttribute {

    public OpCodeValidationOptionalAttribute(params Type[] tail) : base(tail) {
    }

}
