using NoiseEngine.Mathematics;
using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Environments.Nesl;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Nesl.Operations;

public class LoadFieldOperationsTest : NeslTestEnvironment {

    public LoadFieldOperationsTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void LoadAndSetField() {
        const float Value = 20.23f;
        const uint IndexInitial = 1;
        const uint IndexExpected = 3;

        BufferOutputTestHelper<Vector4<float>> helper = CreateBufferOutputTestHelper<Vector4<float>>(true);

        NeslMethodBuilder main = helper.DefineMethod();
        main.AddAttribute(KernelAttribute.Create(Vector3<uint>.One));
        IlGenerator il = main.IlGenerator;

        // Get.
        il.Emit(OpCode.DefVariable, BuiltInTypes.Float32);
        il.Emit(OpCode.LoadField, 1u, 0u, IndexInitial);

        // Set.
        il.Emit(OpCode.SetField, 0u, IndexExpected, 1u);

        il.Emit(OpCode.Return);

        // Assert.
        Vector4<float>[] initialValues = new Vector4<float>[] { new Vector4<float>(65, Value, 7, 33) };
        Vector4<float>[] expectedValues = new Vector4<float>[] { initialValues[0] with { W = Value } };

        helper.ExecuteAndAssert(initialValues, expectedValues);
    }

}
