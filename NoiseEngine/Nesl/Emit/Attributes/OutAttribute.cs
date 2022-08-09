using System;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class OutAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(OutAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Field;

    /// <summary>
    /// Creates new <see cref="OutAttribute"/>.
    /// </summary>
    /// <returns><see cref="OutAttribute"/> with given parameters.</returns>
    public static OutAttribute Create() {
        return new OutAttribute {
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
