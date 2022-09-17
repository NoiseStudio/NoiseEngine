namespace NoiseEngine.CodeGenerators.Interop;

internal readonly struct MarshalParameter {

    public string MarshalledParameterName { get; }
    public string MarshalledType { get; }

    public MarshalParameter(string marshalledParameterName, string marshalledType) {
        MarshalledParameterName = marshalledParameterName;
        MarshalledType = marshalledType;
    }

}
