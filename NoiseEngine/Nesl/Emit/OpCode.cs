namespace NoiseEngine.Nesl.Emit;

public enum OpCode : ushort {
    // Branch operations.
    [OpCodeValidation(typeof(uint), typeof(NeslMethod), typeof(uint[]))]
    Call,
    [OpCodeValidation]
    Return,

    // Def operations.
    [OpCodeValidation(typeof(NeslType))]
    DefVariable,

    // Load operations.
    [OpCodeValidation(typeof(uint), typeof(uint))]
    Load,
    [OpCodeValidation(typeof(uint), typeof(float))]
    LoadFloat32
}
