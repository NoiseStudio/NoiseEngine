namespace NoiseEngine.InternalGenerator.Shared.Interop;

internal readonly struct MarshalOutput {

    public string UnmarshalledParameterName { get; }
    public string MarshalledParameterName { get; }
    public string UnmarshalledType { get; }

    public MarshalOutput(string unmarshalledParameterName, string marshalledParameterName, string unmarshalledType) {
        UnmarshalledParameterName = unmarshalledParameterName;
        MarshalledParameterName = marshalledParameterName;
        UnmarshalledType = unmarshalledType;
    }

}
