using NoiseEngine.Nesl;

namespace NoiseEngine.Rendering;

public class Shader {

    public GraphicsDevice Device { get; }
    public NeslType ClassData { get; }

    public Shader(GraphicsDevice device, NeslType classData) {
        Device = device;
        ClassData = classData;
    }

}
