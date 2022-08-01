namespace NoiseEngine.Nesl.Emit;

public enum OpCode : ushort {
    [OpCodeValidation(OpCodeTail.UInt8)]
    LoadArg,
    [OpCodeValidation]
    Return
}
