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
    /// Checks if that properties have valid values.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when attribute properties are valid; otherwise <see langword="false"/>.
    /// </returns>
    public override bool CheckIsValid() {
        return CheckIfValidFullName(ExpectedFullName) &&
            CheckIfValidTargets(ExpectedTargets) &&
            CheckIfValidBytesLength(-1);
    }

}
