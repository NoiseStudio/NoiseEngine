using NoiseEngine.Collections;

namespace NoiseEngine.Nesl;

internal readonly record struct NeslMethodIdentifier(string Name, ComparableArray<NeslType> ParameterTypes);
