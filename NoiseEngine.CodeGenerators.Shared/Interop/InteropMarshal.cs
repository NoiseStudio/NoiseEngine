using System;

namespace NoiseEngine.CodeGenerators.Shared.Interop;

internal abstract class InteropMarshal {

    internal const string MarshalContinuation = "/*<<< Marshal-Continuation >>>*/";

    public abstract string MarshallingType { get; }
    public abstract string UnmarshallingType { get; }
    public virtual bool IsAdvanced { get; } = false;

    public string GenericRawString { get; private set; } = string.Empty;

    public abstract string Marshall(string unmarshaledParameterName, out string marshaledParameterName);
    public abstract string Unmarshall(string marshaledParameterName, out string unmarshaledParamterName);

    internal void SetGenericRawString(string genericRawString) {
        GenericRawString = genericRawString;
    }

    internal static string CreateUniqueVariableName() {
        return "v" + Guid.NewGuid().ToString("N");
    }

}
