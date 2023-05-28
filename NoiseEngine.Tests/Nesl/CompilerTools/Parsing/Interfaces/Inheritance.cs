using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Interfaces;

public class Inheritance : NeslParsingTestEnvironment {

    [Fact]
    public void Compact() {
        NeslAssembly assembly = CompileSingle(@"
            interface IFoo {}
            interface IBar : IFoo {}

            struct HelloWorld : IBar {
            }
        ");

        NeslType type = assembly.Types.Single(x => x.Name == "HelloWorld");
        Assert.Equal(2, type.Interfaces.Count());
        IEnumerable<NeslType> interfaces = type.Interfaces.OrderBy(x => x.Name);
        Assert.Equal("IBar", interfaces.First().Name);
        Assert.Equal("IFoo", interfaces.Skip(1).First().Name);
    }

}
