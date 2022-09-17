namespace NoiseEngine.CodeGenerators.Shared.Interop;

internal readonly struct MarshalOutput {

    public string UnmarshaledParameterName { get; }
    public string MarshaledParameterName { get; }
    public string UnmarshaledType { get; }

    public MarshalOutput(string unmarshaledParameterName, string marshaledParameterName, string unmarshaledType) {
        UnmarshaledParameterName = unmarshaledParameterName;
        MarshaledParameterName = marshaledParameterName;
        UnmarshaledType = unmarshaledType;
    }

}
