using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NoiseEngine.Jobs;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct ArchetypeColumn<T1, T2> {

    public readonly T1 Element1;
    public readonly T2 Element2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ArchetypeColumn(T1 element1, T2 element2) {
        Element1 = element1;
        Element2 = element2;
    }

    public static nint GetSize() {
        return Unsafe.SizeOf<ArchetypeColumn<T1, T2>>();
    }

    public static unsafe ArchetypeColumn<nint, nint> GetOffsets() {
        ArchetypeColumn<T1, T2> i = default;
#pragma warning disable CS8500
        byte* a = (byte*)&i;
        return new ArchetypeColumn<nint, nint>((nint)((byte*)&i.Element1 - a), (nint)((byte*)&i.Element2 - a));
#pragma warning restore CS8500
    }

}
