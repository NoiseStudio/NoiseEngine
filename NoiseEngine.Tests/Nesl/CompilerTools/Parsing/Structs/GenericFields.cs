using NoiseEngine.Nesl;
using NoiseEngine.Tests.Environments;
using System.Linq;

namespace NoiseEngine.Tests.Nesl.CompilerTools.Parsing.Structs;

public class GenericFields : NeslParsingTestEnvironment {

    [Fact]
    public void DefineSingle() {
        NeslAssembly assembly = CompileSingle(@"
            internal struct Mazdamer<T> {
                public T Value;
            }

            struct Mock {}
        ");

        NeslType? a = assembly.Types.SingleOrDefault(x => x.Name == "Mazdamer");
        Assert.NotNull(a);
        NeslType? b = assembly.Types.SingleOrDefault(x => x.Name == "Mock");
        Assert.NotNull(b);

        NeslType maked = a!.MakeGeneric(b!);
        Assert.Single(maked.Fields);
        Assert.Equal(b, maked.Fields[0].FieldType);
    }

    [Fact]
    public void DefineSeveral() {
        NeslAssembly assembly = CompileSingle(@"
            local struct Mazdamer<T1, T2> {
                public T1 A;
                public T2 B;
            }

            struct MockA {}
            struct MockB {}
        ");

        NeslType? a = assembly.Types.SingleOrDefault(x => x.Name == "Mazdamer");
        Assert.NotNull(a);
        NeslType? b = assembly.Types.SingleOrDefault(x => x.Name == "MockA");
        Assert.NotNull(b);
        NeslType? c = assembly.Types.SingleOrDefault(x => x.Name == "MockB");
        Assert.NotNull(c);

        NeslType maked = a!.MakeGeneric(b!, c!);
        Assert.Equal(2, maked.Fields.Count);
        Assert.Equal(b, maked.Fields.Single(x => x.Name == "A").FieldType);
        Assert.Equal(c, maked.Fields.Single(x => x.Name == "B").FieldType);
    }

    [Fact]
    public void DefineCompact() {
        NeslAssembly assembly = CompileSingle(@"
            using System;

            struct Mazdamer<T1, T2> {
                public Compact<T1> Vector;
                public T2 Value;
            }

            struct Compact<T> {
                T Value;
            }

            struct MockA {}
            struct MockB {}
        ");

        NeslType? a = assembly.Types.SingleOrDefault(x => x.Name == "Mazdamer");
        Assert.NotNull(a);
        NeslType? b = assembly.Types.SingleOrDefault(x => x.Name == "MockA");
        Assert.NotNull(b);
        NeslType? c = assembly.Types.SingleOrDefault(x => x.Name == "MockB");
        Assert.NotNull(c);
        NeslType? compact = assembly.Types.SingleOrDefault(x => x.Name == "Compact");
        Assert.NotNull(compact);

        NeslType maked = a!.MakeGeneric(b!, c!);
        Assert.Equal(2, maked.Fields.Count);
        Assert.Equal(compact!.MakeGeneric(b!), maked.Fields.Single(x => x.Name == "Vector").FieldType);
        Assert.Equal(c, maked.Fields.Single(x => x.Name == "Value").FieldType);
    }

}
