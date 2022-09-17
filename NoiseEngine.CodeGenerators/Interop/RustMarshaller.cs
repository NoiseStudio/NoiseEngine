using System;

namespace NoiseEngine.CodeGenerators.Interop;

internal abstract class RustMarshaller {

    internal const string MarshallingContinuation = "/*<<< Marshaling-Continuation >>>*/";

    public abstract string MarshalledType { get; }
    public abstract string UnmarshalledType { get; }
    public virtual bool IsAdvanced { get; }

    public string GenericRawString { get; private set; } = string.Empty;

    public abstract string Marshall(string parameterName, out string newParameterName);
    public abstract string Unmarshall(string parameterName, out string newParameterName);

    internal void SetGenericRawString(string genericRawString) {
        GenericRawString = genericRawString;
    }

    internal static string CreateUniqueVariableName() {
        return "v" + Guid.NewGuid().ToString("N");
    }

}
