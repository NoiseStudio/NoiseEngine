using System;
using System.Collections.Generic;

namespace NoiseEngine.Jobs2;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
    AllowMultiple = true, Inherited = true
)]
public class AppendComponentDefaultAttribute : Attribute {

    public IReadOnlyList<Type> Components { get; }

    public AppendComponentDefaultAttribute(params Type[] components) {
        Components = components;
    }

}
