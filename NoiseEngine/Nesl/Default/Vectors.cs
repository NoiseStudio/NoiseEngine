using System;

namespace NoiseEngine.Nesl.Default;

internal static class Vectors {

    public const string Vector2Name = "System::System.Vector2`1";
    public const string Vector3Name = "System::System.Vector3`1";
    public const string Vector4Name = "System::System.Vector4`1";

    public static NeslType GetVector2(NeslType type) {
        NeslType vector = Manager.Assembly.GetType(Vector2Name) ?? throw new NullReferenceException();
        return vector.MakeGeneric(type);
    }

    public static NeslType GetVector3(NeslType type) {
        NeslType vector = Manager.Assembly.GetType(Vector3Name) ?? throw new NullReferenceException();
        return vector.MakeGeneric(type);
    }

    public static NeslType GetVector4(NeslType type) {
        NeslType vector = Manager.Assembly.GetType(Vector4Name) ?? throw new NullReferenceException();
        return vector.MakeGeneric(type);
    }

}
