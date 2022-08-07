using System;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class StaticAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(StaticAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Field | AttributeTargets.Method;

    /// <summary>
    /// Creates new <see cref="StaticAttribute"/>.
    /// </summary>
    /// <returns><see cref="StaticAttribute"/> with given parameters.</returns>
    public static StaticAttribute Create() {
        return new StaticAttribute {
            FullName = ExpectedFullName,
            Targets = ExpectedTargets,
        };
    }

    /// <summary>
    /// Asserts that properties have valid values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Not all properties have valid values.</exception>
    public override void AssertValid() {
        AssertValidFullName(ExpectedFullName);
        AssertValidTargets(ExpectedTargets);
        AssertValidBytesLength(-1);
    }

}
