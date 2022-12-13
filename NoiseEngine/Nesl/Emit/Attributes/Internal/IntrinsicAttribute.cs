namespace NoiseEngine.Nesl.Emit.Attributes.Internal;

internal class IntrinsicAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(IntrinsicAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Method;

    /// <summary>
    /// Creates new <see cref="IntrinsicAttribute"/>.
    /// </summary>
    /// <returns><see cref="IntrinsicAttribute"/> with given parameters.</returns>
    public static IntrinsicAttribute Create() {
        return new IntrinsicAttribute {
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
