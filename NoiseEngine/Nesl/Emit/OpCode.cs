namespace NoiseEngine.Nesl.Emit;

public enum OpCode : ushort {
    [OpCodeValidation(typeof(byte))]
    LoadArg,
    [OpCodeValidation(typeof(NeslField))]
    LoadField,
    [OpCodeValidation(typeof(uint))]
    LoadUInt32,
    [OpCodeValidation(typeof(ulong))]
    LoadUInt64,
    [OpCodeValidation(typeof(float))]
    LoadFloat32,

    [OpCodeValidation(typeof(NeslType))]
    SetElement,

    [OpCodeValidation]
    Add,
    [OpCodeValidation]
    Sub,
    [OpCodeValidation]
    Mul,
    [OpCodeValidation]
    Div,

    [OpCodeValidation(typeof(NeslMethod))]
    Call,
    [OpCodeValidation]
    Return
}
