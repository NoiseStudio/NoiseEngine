using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class Generic : NeslParsingTestEnvironment {

    [Fact]
    public void DefineSingle() {
        NeslType type = CompileSingle(@"
            interface Ugh<T1> {
                T1 Get();
            }
        ").Types.Single();

        Assert.Single(type.GenericTypeParameters);
        Assert.Equal("T1", type.GenericTypeParameters.First().Name);
    }

    [Fact]
    public void DefineSeveral() {
        NeslType type = CompileSingle(@"
            interface Ugh<T1, T2> {
                T1 Process(T2 input);
            }
        ").Types.Single();

        Assert.Equal(2, type.GenericTypeParameters.Count());
        Assert.Equal("T1", type.GenericTypeParameters.First().Name);
        Assert.Equal("T2", type.GenericTypeParameters.Skip(1).First().Name);
    }

}
