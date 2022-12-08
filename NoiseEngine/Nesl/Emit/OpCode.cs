namespace NoiseEngine.Nesl.Emit;

public enum OpCode : ushort {
    [OpCodeValidation(typeof(byte))]
    LoadArg,
    [OpCodeValidation(typeof(uint), typeof(NeslField))]
    LoadField,
    [OpCodeValidation(typeof(uint), typeof(uint))]
    LoadUInt32,
    [OpCodeValidation(typeof(ulong))]
    LoadUInt64,
    [OpCodeValidation(typeof(uint), typeof(float))]
    LoadFloat32,

    [OpCodeValidation(typeof(NeslField), typeof(uint))]
    SetField,
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
    Return,

    [OpCodeValidation(typeof(NeslType), typeof(uint))]
    DefVariable,
}
