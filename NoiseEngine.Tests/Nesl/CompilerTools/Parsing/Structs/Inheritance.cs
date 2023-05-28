using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class Inheritance : NeslParsingTestEnvironment {

    [Fact]
    public void Single() {
        NeslAssembly assembly = CompileSingle(@"
            interface IFoo {}

            struct HelloWorld : IFoo {
            }
        ");

        NeslType type = assembly.Types.Single(x => x.Name == "HelloWorld");
        Assert.Single(type.Interfaces);
        Assert.Equal("IFoo", type.Interfaces.Single().Name);
    }

    [Fact]
    public void Several() {
        NeslAssembly assembly = CompileSingle(@"
            interface IFoo {}
            interface IBar {}

            struct HelloWorld : IFoo, IBar {
            }
        ");

        NeslType type = assembly.Types.Single(x => x.Name == "HelloWorld");
        Assert.Equal(2, type.Interfaces.Count());
        IEnumerable<NeslType> interfaces = type.Interfaces.OrderBy(x => x.Name);
        Assert.Equal("IBar", interfaces.First().Name);
        Assert.Equal("IFoo", interfaces.Skip(1).First().Name);
    }

    [Fact]
    public void SeveralGenerics() {
        NeslAssembly assembly = CompileSingle(@"
            interface IFoo {}
            interface IBar<T> {}

            struct HelloWorld : IFoo, IBar<HelloWorld> {
            }
        ");

        NeslType type = assembly.Types.Single(x => x.Name == "HelloWorld");
        Assert.Equal(2, type.Interfaces.Count());
        IEnumerable<NeslType> interfaces = type.Interfaces.OrderBy(x => x.Name);
        Assert.Equal("IBar`1", interfaces.First().Name);
        Assert.Equal("IFoo", interfaces.Skip(1).First().Name);

        Assert.Single(interfaces.First().GenericMakedTypeParameters);
        Assert.Equal("HelloWorld", interfaces.First().GenericMakedTypeParameters.Single().Name);
    }

}
