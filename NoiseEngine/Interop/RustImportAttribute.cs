using System;

namespace NoiseEngine.Interop;

[AttributeUsage(AttributeTargets.Method)]
internal class RustImportAttribute : Attribute {

    public string EntryPoint { get; }
    public string DllName { get; }

    public RustImportAttribute(string entryPoint, string dllName = InteropConstants.DllName) {
        EntryPoint = entryPoint;
        DllName = dllName;
    }

}
