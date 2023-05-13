namespace NoiseEngine.Nesl.CompilerTools.Parsing.Constructors;

internal readonly record struct ValueData(NeslType Type, uint Id) {

    public static ValueData Invalid => new ValueData(null!, uint.MaxValue);

}
