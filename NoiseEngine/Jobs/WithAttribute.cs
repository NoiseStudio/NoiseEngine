using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class WithAttribute : Attribute {

    public IReadOnlyList<Type> Components { get; }

    public WithAttribute(params Type[] components) {
        Components = components;
    }

}
