using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Nesl.Operations;

public class LoadOperationsTest : NeslTestEnvironment {

    public LoadOperationsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Load() {
        const float Value = 0.69201f;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>();

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = main.IlGenerator;

        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadFloat32, 1u, Value);
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.Load, 2u, 1u);
        il.Emit(OpCode.Load, 0u, 2u);
        il.Emit(OpCode.Return);

        helper.ExecuteAndAssert(null, Value);
    }

}
