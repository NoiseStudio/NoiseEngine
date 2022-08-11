using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;

namespace NoiseEngine.Nesl;

internal readonly record struct NeslEntryPoint(NeslMethod Method, ExecutionModel ExecutionModel);
