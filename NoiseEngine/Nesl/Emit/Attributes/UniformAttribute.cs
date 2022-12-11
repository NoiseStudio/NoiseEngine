namespace NoiseEngine.Nesl.Emit.Attributes;

public class UniformAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(UniformAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Field;

    /// <summary>
    /// Creates new <see cref="UniformAttribute"/>.
    /// </summary>
    /// <returns><see cref="UniformAttribute"/> with given parameters.</returns>
    public static UniformAttribute Create() {
        return new UniformAttribute {
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
