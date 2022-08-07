namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

/// <summary>
/// https://www.khronos.org/registry/SPIR-V/specs/1.2/SPIRV.html#Addressing_Model
/// </summary>
internal enum AddressingModel : uint {
    Logical = 0,
    Physical32 = 1,
    Physical64 = 2
}
