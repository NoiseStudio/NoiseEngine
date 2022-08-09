using System;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class InAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(InAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Field;

    /// <summary>
    /// Creates new <see cref="InAttribute"/>.
    /// </summary>
    /// <returns><see cref="InAttribute"/> with given parameters.</returns>
    public static InAttribute Create() {
        return new InAttribute {
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
