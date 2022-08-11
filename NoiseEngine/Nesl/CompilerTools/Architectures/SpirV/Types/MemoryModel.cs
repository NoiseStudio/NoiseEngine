namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

/// <summary>
/// https://www.khronos.org/registry/SPIR-V/specs/1.2/SPIRV.html#Memory_Model
/// </summary>
internal enum MemoryModel : uint {
    Simple = 0,
    Glsl450 = 1,
    OpenCl = 2
}
