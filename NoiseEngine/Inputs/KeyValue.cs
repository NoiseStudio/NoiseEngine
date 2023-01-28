using System.Runtime.InteropServices;

namespace NoiseEngine.Inputs;

[StructLayout(LayoutKind.Sequential)]
internal readonly record struct KeyValue(KeyModifier Modifier, KeyState State);
