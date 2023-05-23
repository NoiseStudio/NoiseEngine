using System;

namespace NoiseEngine.Nesl.Default;

internal static class Matrices {

    public const string Matrix4x4Name = "System::System.Matrix4x4`1";

    public static NeslType GetMatrix4x4(NeslType type) {
        NeslType matrix = Manager.Assembly.GetType(Matrix4x4Name) ?? throw new NullReferenceException();
        return matrix.MakeGeneric(type);
    }

}
