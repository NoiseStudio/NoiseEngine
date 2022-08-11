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
