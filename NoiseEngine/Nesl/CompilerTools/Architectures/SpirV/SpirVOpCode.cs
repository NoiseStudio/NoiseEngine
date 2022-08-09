using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal enum SpirVOpCode : ushort {
    [OpCodeValidation(typeof(uint), typeof(uint))]
    OpMemoryModel = 14,
    [OpCodeValidation(typeof(uint), typeof(SpirVId), typeof(string))]
    OpEntryPoint = 15,
    [OpCodeValidation(typeof(SpirVId), typeof(uint))]
    OpExecutionMode = 16,
    [OpCodeValidation(typeof(uint))]
    OpCapability = 17,

    [OpCodeValidation(typeof(SpirVId))]
    OpTypeVoid = 19,
    [OpCodeValidation(typeof(SpirVId), typeof(uint))]
    OpTypeFloat = 22,
    [OpCodeValidation(typeof(SpirVId), typeof(uint), typeof(SpirVId))]
    OpTypePointer = 32,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId))]
    OpTypeFunction = 33,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(uint), typeof(SpirVId))]
    OpFunction = 54,
    [OpCodeValidation]
    OpFunctionEnd = 56,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(uint))]
    OpVariable = 59,

    [OpCodeValidation(typeof(SpirVId))]
    OpLabel = 248,
    [OpCodeValidation]
    OpReturn = 253,
}
