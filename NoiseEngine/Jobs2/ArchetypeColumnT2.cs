﻿using System.Runtime.InteropServices;

namespace NoiseEngine.Jobs2;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ArchetypeColumn<T1, T2> {

    private readonly T1 t1;
    private readonly T2 t2;

}