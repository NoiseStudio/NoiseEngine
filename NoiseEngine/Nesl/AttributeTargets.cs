using System;

namespace NoiseEngine.Nesl;

[Flags]
public enum AttributeTargets : uint {
    Type = 1 << 0,

    Field = 1 << 1,

    Method = 1 << 2,
    Parameter = 1 << 3,
    ReturnValue = 1 << 4,

    All = Type | Field | Method | Parameter | ReturnValue
}
