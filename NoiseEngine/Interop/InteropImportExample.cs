using System;

namespace NoiseEngine.Interop;

internal static partial class InteropImportExample {

    [InteropImport("interop_interop_span_test_unmanaged_read")]
    private static partial Span<int> InteropUnmanagedRead(Span<int> span, int index);

    [InteropImport("interop_interop_span_test_unmanaged_write")]
    private static partial int InteropUnmanagedWrite(int index);

}
