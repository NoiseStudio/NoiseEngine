using NoiseEngine.Nesl;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Nesl.Operations;

public class BranchOperationsTest : NeslTestEnvironment {

    public BranchOperationsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Call() {
        const float Value = 7.7f;

        BufferOutputTestHelper<float> helper = CreateBufferOutputTestHelper<float>();

        // Set method.
        NeslMethodBuilder set = helper.DefineMethod(null, BuiltInTypes.Float32);
        IlGenerator il = set.IlGenerator;

        il.Emit(OpCode.Load, 0u, 1u);
        il.Emit(OpCode.Return);

        // Main method.
        helper.Assert(il => {
            il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
            il.Emit(OpCode.LoadFloat32, 1u, Value);
            il.Emit(OpCode.Call, uint.MaxValue, set, stackalloc uint[] { 1u });
            il.Emit(OpCode.Return);
        }, Value);
    }

}
