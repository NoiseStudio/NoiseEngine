namespace NoiseEngine.CodeGenerators.Shared.Interop.InteropMarshalling;

internal class SpanMarshal : InteropMarshal {

    public override string MarshallingType => "System.Span";
    public override string UnmarshallingType => "NoiseEngine.Interop.InteropMarshalling.InteropSpan";
    public override bool IsAdvanced => true;

    public override string Marshall(string unmarshaledParameterName, out string marshaledParameterName) {
        marshaledParameterName = CreateUniqueVariableName();
        string finalType = $"{UnmarshallingType}<{GenericRawString}>";
        string a = CreateUniqueVariableName();

        return @$"
            fixed ({GenericRawString}* {a} = {unmarshaledParameterName}) {{
                {finalType} {marshaledParameterName} = new {finalType}({a}, {unmarshaledParameterName}.Length);

                {MarshalContinuation}
            }}
        ";
    }

    public override string Unmarshall(string marshaledParameterName, out string unmarshaledParamterName) {
        unmarshaledParamterName = CreateUniqueVariableName();
        return $"{MarshallingType}<{GenericRawString}> {unmarshaledParamterName} " +
            $"= {marshaledParameterName}.AsSpan();";
    }

}
