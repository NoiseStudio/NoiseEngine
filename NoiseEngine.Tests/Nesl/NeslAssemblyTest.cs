using NoiseEngine.Nesl.Emit;

namespace NoiseEngine.Tests.Nesl;

public class NeslAssemblyTest {

    [Fact]
    public void GetTypeString() {
        const string TypeName = "Example";

        NeslAssemblyBuilder assembly = TestEmitHelper.NewAssembly();

        Assert.Null(assembly.GetType(TypeName));
        Assert.Empty(assembly.Types);

        NeslTypeBuilder type = assembly.DefineType(TypeName);

        Assert.Equal(type, assembly.GetType(TypeName));
        Assert.Single(assembly.Types);
    }

}
