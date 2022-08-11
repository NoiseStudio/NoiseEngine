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
