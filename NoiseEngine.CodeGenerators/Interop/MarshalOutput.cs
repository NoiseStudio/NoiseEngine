namespace NoiseEngine.CodeGenerators.Interop;

internal readonly struct MarshalOutput {

    public string UnmarshalledParameterName { get; }
    public string MarshalledParameterName { get; }
    public string UnmarshalledType { get; }
    public string MarshalledType { get; }

    public MarshalOutput(
        string unmarshalledParameterName, string marshalledParameterName, string unmarshalledType, string marshalledType
    ) {
        UnmarshalledParameterName = unmarshalledParameterName;
        MarshalledParameterName = marshalledParameterName;
        UnmarshalledType = unmarshalledType;
        MarshalledType = marshalledType;
    }

}
