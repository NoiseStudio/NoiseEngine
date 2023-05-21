namespace NoiseEngine.Nesl;

internal enum NeslTypeUsageKind : byte {
    Normal = 0,
    GenericTypeParameter = 1,
    GenericMaked = 2,
    GenericNotFullyConstructed = 3
}
