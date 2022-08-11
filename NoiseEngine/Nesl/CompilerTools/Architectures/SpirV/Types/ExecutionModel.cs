namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

/// <summary>
/// https://registry.khronos.org/SPIR-V/specs/1.2/SPIRV.html#Execution_Model
/// </summary>
internal enum ExecutionModel : uint {
    Vertex = 0,
    TessellationControl = 1,
    TessellationEvaluation = 2,
    Geometry = 3,
    Fragment = 4,
    GLCompute = 5,
    Kernel = 6
}
