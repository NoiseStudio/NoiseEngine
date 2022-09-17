using System;

namespace NoiseEngine.CodeGenerators.Interop;

internal abstract class InteropMarshal {

    internal const string MarshalContinuation = "/*<<< Marshal-Continuation >>>*/";

    public abstract string MarshallingType { get; }
    public abstract string UnmarshallingType { get; }
    public virtual bool IsAdvanced { get; }

    public string GenericRawString { get; private set; } = string.Empty;

    public abstract string Marshall(string unmarshalledParameterName, out string marshalledParameterName);
    public abstract string Unmarshall(string marshalledParameterName, out string unmarshalledParamterName);

    internal void SetGenericRawString(string genericRawString) {
        GenericRawString = genericRawString;
    }

    internal static string CreateUniqueVariableName() {
        return "v" + Guid.NewGuid().ToString("N");
    }

}
