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
