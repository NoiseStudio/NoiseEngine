using System;

namespace NoiseEngine.Nesl.Default;

internal static class VertexUtils {

    private const string TypeName = $"System::System.{nameof(VertexUtils)}";

    public static NeslMethod Index =>
        Type.GetMethod(NeslOperators.PropertyGet + nameof(Index)) ?? throw new NullReferenceException();

    public static NeslMethod ObjectToClipPos =>
        Type.GetMethod(nameof(ObjectToClipPos)) ?? throw new NullReferenceException();

    private static NeslType Type => Manager.Assembly.GetType(TypeName) ?? throw new NullReferenceException();

}
