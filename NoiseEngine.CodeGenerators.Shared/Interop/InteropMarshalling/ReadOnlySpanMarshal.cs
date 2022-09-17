namespace NoiseEngine.CodeGenerators.Shared.Interop.InteropMarshalling;

internal class ReadOnlySpanMarshal : InteropMarshal {

    public override string MarshalingType => "System.ReadOnlySpan";
    public override string UnmarshalingType => "NoiseEngine.Interop.InteropMarshalling.InteropReadOnlySpan";
    public override bool IsAdvanced => true;

    public override string Marshal(string unmarshaledParameterName, out string marshaledParameterName) {
        marshaledParameterName = CreateUniqueVariableName();
        string finalType = $"{UnmarshalingType}<{GenericRawString}>";
        string a = CreateUniqueVariableName();

        return @$"
            fixed ({GenericRawString}* {a} = {unmarshaledParameterName}) {{
                {finalType} {marshaledParameterName} = new {finalType}({a}, {unmarshaledParameterName}.Length);

                {MarshalContinuation}
            }}
        ";
    }

    public override string Unmarshal(string marshaledParameterName, out string unmarshaledParameterName) {
        unmarshaledParameterName = CreateUniqueVariableName();
        return $"{MarshalingType}<{GenericRawString}> {unmarshaledParameterName} " +
            $"= {marshaledParameterName}.AsSpan();";
    }

}
