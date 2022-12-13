namespace NoiseEngine.Nesl.Emit;

public enum OpCode : ushort {
    [OpCodeValidation(typeof(NeslType))]
    DefVariable,

    [OpCodeValidation(typeof(uint), typeof(uint))]
    Load,
    [OpCodeValidation(typeof(uint), typeof(float))]
    LoadFloat32,

    [OpCodeValidation]
    Return
}
