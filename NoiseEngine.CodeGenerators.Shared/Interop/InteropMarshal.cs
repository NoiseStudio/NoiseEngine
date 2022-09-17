using System;

namespace NoiseEngine.CodeGenerators.Shared.Interop;

internal abstract class InteropMarshal {

    internal const string MarshalContinuation = "/*<<< Marshal-Continuation >>>*/";

    public abstract string MarshallingType { get; }
    public abstract string UnmarshallingType { get; }
    public virtual bool IsAdvanced => false;

    public string GenericRawString { get; private set; } = string.Empty;

    public abstract string Marshal(string unmarshalledParameterName, out string marshalledParameterName);
    public abstract string Unmarshal(string marshalledParameterName, out string unmarshalledParameterName);

    internal void SetGenericRawString(string genericRawString) {
        GenericRawString = genericRawString;
    }

    internal static string CreateUniqueVariableName() {
        return "v" + Guid.NewGuid().ToString("N");
    }

}
