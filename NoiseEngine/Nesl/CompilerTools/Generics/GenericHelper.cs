using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal static class GenericHelper {

    public static NeslType GetFinalType(
        NeslType oldType, NeslType newType, NeslType currentType,
        IDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        if (currentType is NeslGenericTypeParameter genericTypeParameter)
            return targetTypes[genericTypeParameter];
        if (currentType is NotFullyConstructedGenericNeslType notFullyGenericMakedType) {
            return notFullyGenericMakedType.MakeGeneric(notFullyGenericMakedType.TypeArguments
                .Select(x => x is NeslGenericTypeParameter genericTypeParameter ?
                    targetTypes[genericTypeParameter] : x)
                .ToArray()
            );
        }
        if (!currentType.IsGeneric)
            return currentType;
        if (oldType == currentType)
            return newType;

        NeslType[] typeParameters = currentType.GenericTypeParameters.ToArray();
        bool changed = false;

        for (int i = 0; i < typeParameters.Length; i++) {
            if (targetTypes.TryGetValue((NeslGenericTypeParameter)typeParameters[i], out NeslType? n)) {
                typeParameters[i] = n;
                changed = true;
            }
        }

        if (changed)
            return currentType.MakeGeneric(typeParameters);
        return currentType;
    }

    public static NeslAttribute[] RemoveGenericsFromAttributes(
        IEnumerable<NeslAttribute> attributes, IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        return attributes.Select(x => x.RemoveGenericsInternal(targetTypes)).ToArray();
    }

}
