using System;

namespace NoiseEngine.CodeGenerators.Shared.Interop;

internal abstract class InteropMarshal {

    internal const string MarshalContinuation = "/*<<< Marshal-Continuation >>>*/";

    public abstract string MarshalingType { get; }
    public abstract string UnmarshalingType { get; }
    public virtual bool IsAdvanced => false;

    public string GenericRawString { get; private set; } = string.Empty;

    public abstract string Marshal(string unmarshaledParameterName, out string marshaledParameterName);
    public abstract string Unmarshal(string marshaledParameterName, out string unmarshaledParameterName);

    internal void SetGenericRawString(string genericRawString) {
        GenericRawString = genericRawString;
    }

    internal static string CreateUniqueVariableName() {
        return "v" + Guid.NewGuid().ToString("N");
    }

}
