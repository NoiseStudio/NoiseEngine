namespace NoiseEngine.CodeGenerators.Interop.InteropMarshalling;

internal class SpanMarshal : InteropMarshal {

    public override string MarshallingType => "System.Span";
    public override string UnmarshallingType => "NoiseEngine.Interop.InteropSpan";
    public override bool IsAdvanced => true;

    public override string Marshall(string unmarshalledParameterName, out string marshalledParameterName) {
        marshalledParameterName = CreateUniqueVariableName();
        string finalType = $"{UnmarshallingType}<{GenericRawString}>";
        string a = CreateUniqueVariableName();

        return @$"
            fixed ({GenericRawString}* {a} = {unmarshalledParameterName}) {{
                {finalType} {marshalledParameterName} = new {finalType}({a}, {unmarshalledParameterName}.Length);

                {MarshalContinuation}
            }}
        ";
    }

    public override string Unmarshall(string marshalledParameterName, out string unmarshalledParamterName) {
        unmarshalledParamterName = CreateUniqueVariableName();
        return $"{MarshallingType}<{GenericRawString}> {unmarshalledParamterName} = {marshalledParameterName};";
    }

}
