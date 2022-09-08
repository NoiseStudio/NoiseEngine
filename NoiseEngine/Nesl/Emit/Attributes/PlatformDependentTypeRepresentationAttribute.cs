using NoiseEngine.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class PlatformDependentTypeRepresentationAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(PlatformDependentTypeRepresentationAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Type;

    public string? CilTargetName => AttributeHelper.ReadString(Bytes.AsSpan());
    public string? SpirVTargetName => AttributeHelper.ReadString(AttributeHelper.JumpToNextBytes(Bytes.AsSpan()));

    /// <summary>
    /// Creates new <see cref="PlatformDependentTypeRepresentationAttribute"/>.
    /// </summary>
    /// <param name="cilTargetName">Target name in CIL.</param>
    /// <param name="spirVTargetName">Target name in SPIR-V.</param>
    /// <returns><see cref="PlatformDependentTypeRepresentationAttribute"/> with given parameters.</returns>
    public static PlatformDependentTypeRepresentationAttribute Create(string? cilTargetName, string? spirVTargetName) {
        FastList<byte> buffer = new FastList<byte>();

        AttributeHelper.WriteString(buffer, cilTargetName);
        AttributeHelper.WriteString(buffer, spirVTargetName);

        return new PlatformDependentTypeRepresentationAttribute {
            FullName = ExpectedFullName,
            Targets = ExpectedTargets,
            Bytes = buffer.ToImmutableArray()
        };
    }

    /// <summary>
    /// Checks if that properties have valid values.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when attribute properties are valid; otherwise <see langword="false"/>.
    /// </returns>
    public override bool CheckIsValid() {
        return CheckIfValidFullName(ExpectedFullName) && CheckIfValidTargets(ExpectedTargets);
    }

    /// <summary>
    /// Removes generics from this <see cref="NeslAttribute"/> with given <paramref name="targetTypes"/>.
    /// </summary>
    /// <param name="targetTypes">
    /// <see cref="IReadOnlyDictionary{TKey, TValue}"/> where TKey is original generic type and TValue is target type.
    /// </param>
    /// <returns>Returns new or this <see cref="NeslAttribute"/> without generics.</returns>
    protected override NeslAttribute RemoveGenerics(
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        bool isEquals = true;

        string? cilTargetName = ReplaceGenericsInTargetName(CilTargetName, targetTypes, true, ref isEquals);
        string? spirVTargetName = ReplaceGenericsInTargetName(SpirVTargetName, targetTypes, true, ref isEquals);

        if (isEquals)
            return this;
        return Create(cilTargetName, spirVTargetName);
    }

    private string? ReplaceGenericsInTargetName(
        string? targetName, IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes, bool isCilTarget,
        ref bool isEquals
    ) {
        if (targetName is null)
            return null;

        StringBuilder builder = new StringBuilder(targetName);

        foreach (NeslGenericTypeParameter genericTypeParameter in targetTypes.Keys) {
            NeslType type = targetTypes[genericTypeParameter];

            PlatformDependentTypeRepresentationAttribute? b = type.Attributes
                .OfType<PlatformDependentTypeRepresentationAttribute>().FirstOrDefault();

            string name;
            if (b is null)
                name = type.Name;
            else
                name = (isCilTarget ? b.CilTargetName : b.SpirVTargetName) ?? type.Name;

            builder.Replace($"{{{genericTypeParameter.Name}}}", name);
        }

        if (builder.Equals(targetName))
            return targetName;

        isEquals = false;
        return builder.ToString();
    }

}
