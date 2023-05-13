namespace NoiseEngine.InternalGenerator.Shared.Interop;

internal readonly struct MarshalParameter {

    public string MarshalledParameterName { get; }
    public string MarshalledType { get; }
    public bool IsIn { get; }

    public MarshalParameter(string marshalledParameterName, string marshalledType, bool isIn) {
        MarshalledParameterName = marshalledParameterName;
        MarshalledType = marshalledType;
        IsIn = isIn;
    }

}
