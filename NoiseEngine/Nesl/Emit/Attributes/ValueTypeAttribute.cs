using System;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class ValueTypeAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(ValueTypeAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Type;

    /// <summary>
    /// Creates new <see cref="ValueTypeAttribute"/>.
    /// </summary>
    /// <returns><see cref="ValueTypeAttribute"/> with given parameters.</returns>
    public static ValueTypeAttribute Create() {
        return new ValueTypeAttribute {
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
