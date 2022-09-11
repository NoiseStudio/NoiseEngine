using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal static class GenericHelper {

    public static NeslType GetFinalType(
        NeslType currentType, IDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        if (currentType is NeslGenericTypeParameter genericTypeParameter) {
            return targetTypes[genericTypeParameter];
        } else if (currentType is NotFullyConstructedGenericNeslType notFullyGenericMakedType) {
            return notFullyGenericMakedType.MakeGeneric(notFullyGenericMakedType.TypeArguments
                .Select(x => x is NeslGenericTypeParameter genericTypeParameter ?
                    targetTypes[genericTypeParameter] : x)
                .ToArray()
            );
        } else {
            return currentType;
        }
    }

    public static NeslAttribute[] RemoveGenericsFromAttributes(
        IEnumerable<NeslAttribute> attributes, IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        return attributes.Select(x => x.RemoveGenericsInternal(targetTypes)).ToArray();
    }

}
