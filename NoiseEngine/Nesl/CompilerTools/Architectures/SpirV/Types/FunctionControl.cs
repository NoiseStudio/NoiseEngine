namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

/// <summary>
/// https://registry.khronos.org/SPIR-V/specs/1.2/SPIRV.html#Function_Control
/// </summary>
internal enum FunctionControl : uint {
    None = 0,
    Inline = 1,
    DontInline = 2,
    Pure = 4,
    Const = 8
}
