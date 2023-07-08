using System;

namespace NoiseEngine.Nesl.Default;

internal static class Textures {

    public const string Texture2DName = "System::System.Texture2D`1";

    public static NeslType GetTexture2D(NeslType type) {
        NeslType buffer = Manager.Assembly.GetType(Texture2DName) ?? throw new NullReferenceException();
        return buffer.MakeGeneric(type);
    }

}
