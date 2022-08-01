namespace NoiseEngine.Nesl.Emit;

public enum OpCode : ushort {
    [OpCodeValidation(typeof(byte))]
    LoadArg,
    [OpCodeValidation(typeof(float))]
    LoadFloat32,

    [OpCodeValidation(typeof(NeslMethod))]
    Call,
    [OpCodeValidation]
    Return
}
