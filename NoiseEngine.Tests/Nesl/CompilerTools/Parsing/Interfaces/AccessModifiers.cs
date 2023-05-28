using NoiseEngine.Tests.Environments;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class AccessModifiers : NeslParsingTestEnvironment {

    [Fact]
    public void Default() {
        CompileSingle(@"
            interface Kkard2 {}
        ");
    }

    [Fact]
    public void Local() {
        CompileSingle(@"
            local interface Xori {}
        ");
    }

    [Fact]
    public void Internal() {
        CompileSingle(@"
            internal interface Vixen {}
        ");
    }

    [Fact]
    public void Public() {
        CompileSingle(@"
            public interface Pp {}
        ");
    }

    [Fact]
    public void NotExists() {
        CompileSingleThrowAny(@"
            NotExistsAccessModifier interface TerQ {}
        ");
    }

}
