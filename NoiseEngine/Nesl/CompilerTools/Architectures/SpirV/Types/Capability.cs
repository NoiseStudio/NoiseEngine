namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

/// <summary>
/// https://www.khronos.org/registry/SPIR-V/specs/1.2/SPIRV.html#Capability
/// </summary>
internal enum Capability : uint {
    Matrix = 0,
    Shader = 1,
    Geometry = 2,
    Tessellation = 3,
    Addresses = 4,
    Linkage = 5,
    Kernel = 6
}
