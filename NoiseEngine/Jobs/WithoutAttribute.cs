using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class WithoutAttribute : Attribute {

    public IReadOnlyList<Type> Components { get; }

    public WithoutAttribute(params Type[] components) {
        Components = components;
    }

}
