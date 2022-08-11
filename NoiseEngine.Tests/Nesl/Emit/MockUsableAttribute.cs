using NoiseEngine.Nesl;

namespace NoiseEngine.Tests.Nesl.Emit;

internal class MockUsableAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(MockUsableAttribute);
    private const AttributeTargets ExpectedTargets = (AttributeTargets)uint.MaxValue;

    public static MockUsableAttribute Create() {
        return new MockUsableAttribute {
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
