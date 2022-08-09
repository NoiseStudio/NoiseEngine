using NoiseEngine.Nesl.Emit;
using System;

namespace NoiseEngine.Tests.Nesl.Emit;

public class NeslAssemblyBuilderTest {

    [Fact]
    public void DefineAssembly() {
        NeslAssemblyBuilder assembly = NeslAssemblyBuilder.DefineAssembly(nameof(DefineAssembly));
        Assert.Equal(nameof(DefineAssembly), assembly.Name);
    }

    [Fact]
    public void DefineType() {
        const string TypeName = "Example";

        NeslAssemblyBuilder assembly = TestEmitHelper.NewAssembly();

        assembly.DefineType(TypeName);
        Assert.Throws<ArgumentException>(() => assembly.DefineType(TypeName));
    }

}
