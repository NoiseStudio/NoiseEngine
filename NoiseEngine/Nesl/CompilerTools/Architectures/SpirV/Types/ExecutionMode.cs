namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

/// <summary>
/// https://registry.khronos.org/SPIR-V/specs/1.2/SPIRV.html#Execution_Mode
/// </summary>
internal enum ExecutionMode : uint {
    OriginUpperLeft = 7,
    OriginLowerLeft = 8,
    LocalSize = 17
}
