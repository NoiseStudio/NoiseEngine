namespace NoiseEngine.Nesl;

public abstract class NeslMethod {

    public string Name { get; }

    protected NeslMethod(string name) {
       Name = name;
    }

}
