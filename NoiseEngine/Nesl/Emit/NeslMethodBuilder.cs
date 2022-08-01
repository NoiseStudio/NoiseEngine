namespace NoiseEngine.Nesl.Emit;

public class NeslMethodBuilder : NeslMethod {

    public IlGenerator IlGenerator { get; } = new IlGenerator();

    internal NeslMethodBuilder(string name) : base(name) {
    }

}
