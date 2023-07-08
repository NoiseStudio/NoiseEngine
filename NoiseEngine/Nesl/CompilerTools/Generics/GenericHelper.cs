using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal static class GenericHelper {

    public static NeslType GetFinalType(
        NeslType oldType, NeslType newType, NeslType currentType,
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        if (currentType is NeslGenericTypeParameter genericTypeParameter)
            return targetTypes[genericTypeParameter];
        if (currentType is NotFullyConstructedGenericNeslType notFullyGenericMakedType) {
            return notFullyGenericMakedType.MakeGeneric(notFullyGenericMakedType.GenericMakedTypeParameters
                .Select(x => x is NeslGenericTypeParameter genericTypeParameter ?
                    targetTypes[genericTypeParameter] : x)
                .ToArray()
            );
        }
        if (oldType == currentType)
            return newType;

        NeslType[] typeParameters;
        bool changed = false;
        if (currentType.IsGeneric) {
            typeParameters = currentType.GenericTypeParameters.ToArray();

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

        typeParameters = currentType.GenericMakedTypeParameters.ToArray();
        for (int i = 0; i < typeParameters.Length; i++) {
            NeslType r = typeParameters[i];
            NeslType n = GetFinalType(oldType, newType, typeParameters[i], targetTypes);
            if (r != n) {
                typeParameters[i] = n;
                changed = true;
            }
        }

        if (changed)
            return currentType.GenericMakedFrom!.MakeGeneric(typeParameters);
        return currentType;
    }

    public static NeslAttribute[] RemoveGenericsFromAttributes(
        IEnumerable<NeslAttribute> attributes, IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        return attributes.Select(x => x.RemoveGenericsInternal(targetTypes)).ToArray();
    }

}
