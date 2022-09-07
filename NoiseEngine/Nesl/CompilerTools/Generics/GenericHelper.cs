using NoiseEngine.Nesl.Emit.Attributes;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal static class GenericHelper {

    public static NeslType GetFinalType(
        NeslType currentType, IDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        if (currentType is NeslGenericTypeParameter genericTypeParameter) {
            return targetTypes[genericTypeParameter];
        } else if (currentType is NotFullyGenericMakedNeslType notFullyGenericMakedType) {
            return notFullyGenericMakedType.MakeGeneric(notFullyGenericMakedType.TypeArguments
                .Select(x => x is NeslGenericTypeParameter genericTypeParameter ?
                    targetTypes[genericTypeParameter] : x)
                .ToArray()
            );
        } else {
            return currentType;
        }
    }

    public static ImmutableArray<NeslAttribute> RemoveGenericsFromAttributes(
        IEnumerable<NeslAttribute> attributes, IDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        NeslAttribute[] result = attributes.ToArray();

        int i = 0;
        foreach (NeslAttribute attribute in attributes) {
            if (attribute is PlatformDependentTypeRepresentationAttribute platform) {
                result[i] = PlatformDependentTypeRepresentationAttribute.Create(
                    ReplaceGenericsInPlatformTargetName(platform.CilTargetName, targetTypes, true),
                    ReplaceGenericsInPlatformTargetName(platform.SpirVTargetName, targetTypes, false)
                );
            }

            i++;
        }

        return result.ToImmutableArray();
    }

    private static string? ReplaceGenericsInPlatformTargetName(
        string? targetName, IDictionary<NeslGenericTypeParameter, NeslType> targetTypes, bool isCilTarget
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
        return builder.ToString();
    }

}
