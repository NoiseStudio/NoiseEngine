using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class Generic : NeslParsingTestEnvironment {

    [Fact]
    public void DefineSingle() {
        NeslType type = CompileSingle(@"
            struct Ugh<T1> {}
        ").Types.Single();

        Assert.Single(type.GenericTypeParameters);
        Assert.Equal("T1", type.GenericTypeParameters.First().Name);
    }

    [Fact]
    public void DefineSeveral() {
        NeslType type = CompileSingle(@"
            struct Ugh<T1, T2> {}
        ").Types.Single();

        Assert.Equal(2, type.GenericTypeParameters.Count());
        Assert.Equal("T1", type.GenericTypeParameters.First().Name);
        Assert.Equal("T2", type.GenericTypeParameters.Skip(1).First().Name);
    }

    [Fact]
    public void DefineWithoutComma() {
        CompileSingleThrowAny(@"
            struct Ugh<T1 T2> {}
        ");
    }

    [Fact]
    public void DefineWithAdditionalComma() {
        CompileSingleThrowAny(@"
            struct Ugh<T1,,T2> {}
        ");
    }

    [Fact]
    public void DefineWithoutBracket() {
        CompileSingleThrowAny(@"
            struct Ugh<T1 {}
        ");
    }

    [Fact]
    public void DefineWithoutBody() {
        CompileSingleThrowAny(@"
            struct Ugh<> {}
        ");
    }

}
