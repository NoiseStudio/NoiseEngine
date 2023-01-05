using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal enum SpirVOpCode : ushort {
    [OpCodeValidation(typeof(uint), typeof(uint))]
    OpMemoryModel = 14,
    [OpCodeValidation(typeof(uint), typeof(SpirVId), typeof(SpirVLiteral))]
    [OpCodeValidationOptional(typeof(SpirVId[]))]
    OpEntryPoint = 15,
    [OpCodeValidation(typeof(SpirVId), typeof(uint))]
    [OpCodeValidationOptional(typeof(SpirVLiteral))]
    OpExecutionMode = 16,
    [OpCodeValidation(typeof(uint))]
    OpCapability = 17,

    [OpCodeValidation(typeof(SpirVId))]
    OpTypeVoid = 19,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVLiteral), typeof(SpirVLiteral))]
    OpTypeInt = 21,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVLiteral))]
    OpTypeFloat = 22,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVLiteral))]
    OpTypeVector = 23,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId))]
    OpTypeRuntimeArray = 29,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId[]))]
    OpTypeStruct = 30,
    [OpCodeValidation(typeof(SpirVId), typeof(uint), typeof(SpirVId))]
    OpTypePointer = 32,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId[]))]
    OpTypeFunction = 33,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVLiteral))]
    OpConstant = 43,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(uint), typeof(SpirVId))]
    OpFunction = 54,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId))]
    OpFunctionParameter = 55,
    [OpCodeValidation]
    OpFunctionEnd = 56,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId[]))]
    OpFunctionCall = 57,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(uint))]
    OpVariable = 59,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpLoad = 61,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId))]
    OpStore = 62,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId[]))]
    OpAccessChain = 65,

    [OpCodeValidation(typeof(SpirVId), typeof(uint))]
    [OpCodeValidationOptional(typeof(SpirVLiteral))]
    OpDecorate = 71,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVLiteral), typeof(uint))]
    [OpCodeValidationOptional(typeof(SpirVLiteral))]
    OpMemberDecorate = 72,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpSNegate = 126,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFNegate = 127,

    [OpCodeValidation(typeof(SpirVId))]
    OpLabel = 248,
    [OpCodeValidation]
    OpReturn = 253,
    [OpCodeValidation(typeof(SpirVId))]
    OpReturnValue = 254
}
