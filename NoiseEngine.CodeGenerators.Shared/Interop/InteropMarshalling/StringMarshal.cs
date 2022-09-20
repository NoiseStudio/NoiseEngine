namespace NoiseEngine.CodeGenerators.Shared.Interop.InteropMarshalling;

internal class StringMarshal : InteropMarshal {

    public override string MarshallingType => "string";
    public override string UnmarshallingType => "NoiseEngine.Interop.InteropMarshalling.InteropString";

    public override string Marshal(string unmarshalledParameterName, out string marshalledParameterName) {
        marshalledParameterName = CreateUniqueVariableName();
        return $"{UnmarshallingType} {marshalledParameterName} = new {UnmarshallingType}({unmarshalledParameterName});";
    }

    public override string Unmarshal(string marshalledParameterName, out string unmarshalledParameterName) {
        unmarshalledParameterName = CreateUniqueVariableName();
        return $"{MarshallingType} {unmarshalledParameterName} = {marshalledParameterName}.ToString();\n"
            + $"{marshalledParameterName}.Dispose();";
    }

}
