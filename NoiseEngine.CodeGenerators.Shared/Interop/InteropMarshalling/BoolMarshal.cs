namespace NoiseEngine.CodeGenerators.Shared.Interop.InteropMarshalling;

internal class BoolMarshal : InteropMarshal {

    public override string MarshallingType => "bool";

    public override string UnmarshallingType => "NoiseEngine.Interop.InteropBool";

    public override string Marshal(string unmarshalledParameterName, out string marshalledParameterName) {
        marshalledParameterName = CreateUniqueVariableName();
        return $"{UnmarshallingType} {marshalledParameterName} = new {UnmarshallingType}({unmarshalledParameterName});";
    }

    public override string Unmarshal(string marshalledParameterName, out string unmarshalledParameterName) {
        unmarshalledParameterName = CreateUniqueVariableName();
        return $"{MarshallingType} {unmarshalledParameterName} = {marshalledParameterName}.Value;";
    }

}
