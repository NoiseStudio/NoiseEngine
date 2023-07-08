using System.Collections.Generic;

namespace NoiseEngine.Nesl.Serialization;

internal record NeslGenericsInitializeIlContainer(
    SerializedNeslType Type,
    Dictionary<NeslGenericTypeParameter, NeslType> TargetTypes,
    List<(NeslMethod, NeslMethod)> Methods
);
