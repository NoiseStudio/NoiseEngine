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
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVLiteral))]
    OpTypeMatrix = 24,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpTypeArray = 28,
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
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId[]))]
    OpConstantComposite = 44,

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

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId[]))]
    OpCompositeConstruct = 80,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVLiteral))]
    OpCompositeExtract = 81,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFConvert = 115,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpSNegate = 126,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFNegate = 127,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpIAdd = 128,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFAdd = 129,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpISub = 130,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFSub = 131,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpIMul = 132,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFMul = 133,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpUDiv = 134,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpSDiv = 135,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFDiv = 136,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpUMod = 137,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpSRem = 138,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpSMod = 139,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFRem = 140,
    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpFMod = 141,

    [OpCodeValidation(typeof(SpirVId), typeof(SpirVId), typeof(SpirVId), typeof(SpirVId))]
    OpMatrixTimesVector = 145,

    [OpCodeValidation(typeof(SpirVId))]
    OpLabel = 248,
    [OpCodeValidation]
    OpReturn = 253,
    [OpCodeValidation(typeof(SpirVId))]
    OpReturnValue = 254
}
