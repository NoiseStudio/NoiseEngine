using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Emit.Attributes.Internal;

namespace NoiseEngine.Nesl.Default;

internal static class Vertex {

    public static NeslMethod Index { get; }

    public static NeslMethod ObjectToClipPos { get; }

    static Vertex() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.Vertex");

        Index = DefineProperty(type, nameof(Index), BuiltInTypes.Int32);

        ObjectToClipPos = CreateObjectToClipPos(type);
    }

    private static NeslMethod CreateObjectToClipPos(NeslTypeBuilder type) {
        NeslMethodBuilder method = type.DefineMethod(
            nameof(ObjectToClipPos), Vectors.GetVector4(BuiltInTypes.Float32), Vectors.GetVector3(BuiltInTypes.Float32)
        );

        method.AddAttribute(StaticAttribute.Create());
        method.AddAttribute(IntrinsicAttribute.Create());

        return method;
    }

    private static NeslMethod DefineProperty(NeslTypeBuilder type, string name, NeslType returnType) {
        NeslMethodBuilder property = type.DefineMethod(NeslOperators.PropertyGet + name, returnType);

        property.AddAttribute(StaticAttribute.Create());
        property.AddAttribute(IntrinsicAttribute.Create());

        return property;
    }

}
