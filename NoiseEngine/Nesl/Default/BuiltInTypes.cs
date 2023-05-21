using System;

namespace NoiseEngine.Nesl.Default;

internal static class BuiltInTypes {

    public const string UInt32Name = "System::System.UInt32";
    public const string Int32Name = "System::System.Int32";
    public const string Float32Name = "System::System.Float32";
    public const string Float64Name = "System::System.Float64";

    public static NeslType UInt32 => Manager.Assembly.GetType(UInt32Name) ?? throw new NullReferenceException();
    public static NeslType Int32 => Manager.Assembly.GetType(Int32Name) ?? throw new NullReferenceException();
    public static NeslType Float32 => Manager.Assembly.GetType(Float32Name) ?? throw new NullReferenceException();
    public static NeslType Float64 => Manager.Assembly.GetType(Float64Name) ?? throw new NullReferenceException();

}
