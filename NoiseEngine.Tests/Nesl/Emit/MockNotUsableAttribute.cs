using NoiseEngine.Nesl;

namespace NoiseEngine.Tests.Nesl.Emit;

internal class MockNotUsableAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(MockNotUsableAttribute);
    private const AttributeTargets ExpectedTargets = 0;

    public static MockNotUsableAttribute Create() {
        return new MockNotUsableAttribute {
            FullName = ExpectedFullName,
            Targets = ExpectedTargets,
        };
    }

    public override bool CheckIsValid() {
        return CheckIfValidFullName(ExpectedFullName) &&
            CheckIfValidTargets(ExpectedTargets) &&
            CheckIfValidBytesLength(-1);
    }

}
