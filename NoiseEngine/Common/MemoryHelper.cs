using System.Runtime.InteropServices;

namespace NoiseEngine.Common;

/// <summary>
/// Collection of utility helpers memory management.
/// </summary>
internal static class MemoryHelper
{
    
    /// <summary>
    /// Returns the alignment requirement (in bytes) of an unmanaged type.
    /// </summary>
    /// <typeparam name="T">The unmanaged type for which to determine the alignment.</typeparam>
    /// <returns>The alignment requirement (in bytes) of the specified unmanaged type.</returns>
    public static nuint AlignmentOf<T>() where T : unmanaged {
        return (nuint)Marshal.SizeOf<T>();
    }

    /// <summary>
    /// Checks if the provided memory address is considered dangling for a specified unmanaged type.
    /// </summary>
    /// <typeparam name="T">The unmanaged type for which to perform the check.</typeparam>
    /// <param name="address">The memory address to check.</param>
    /// <returns>True if the memory address is equal to the alignment requirement of the specified unmanaged type; otherwise, false.</returns>
    public static bool IsDangling<T>(nuint address) where T : unmanaged {
        return address == AlignmentOf<T>();    
    }
}
