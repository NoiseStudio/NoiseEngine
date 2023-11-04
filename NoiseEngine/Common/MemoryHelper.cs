using System;

namespace NoiseEngine.Common;

/// <summary>
/// Collection of utility helpers memory management.
/// </summary>
internal static class MemoryHelper
{
    public static nuint AlignmentOf<T>() where T : unmanaged
    {
        return (nuint)System.Runtime.InteropServices.Marshal.SizeOf<T>();
    }
    
    public static bool IsDangling<T>(nuint address) where T: unmanaged => address == AlignmentOf<T>();
}