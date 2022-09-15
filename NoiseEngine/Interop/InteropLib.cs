using System.Runtime.InteropServices;

namespace NoiseEngine.Interop;

internal static partial class InteropLib {
    [LibraryImport(InteropConstants.DllName, EntryPoint = "add")]
    public static partial int Add(int left, int right);
}
