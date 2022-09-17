using System;

namespace NoiseEngine.Interop;

[AttributeUsage(AttributeTargets.Method)]
internal class InteropImportAttribute : Attribute {

    public string EntryPoint { get; }
    public string DllName { get; }

    public InteropImportAttribute(string entryPoint, string dllName = InteropConstants.DllName) {
        EntryPoint = entryPoint;
        DllName = dllName;
    }

}
