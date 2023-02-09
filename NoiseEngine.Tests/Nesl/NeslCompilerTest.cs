using NoiseEngine.Nesl;

namespace NoiseEngine.Tests.Nesl;

public class NeslCompilerTest {

    [Fact]
    public void Compile() {
        NeslCompiler.Compile(nameof(Compile), new NeslFile[] { new NeslFile("Path", "uniform") });
    }

}
