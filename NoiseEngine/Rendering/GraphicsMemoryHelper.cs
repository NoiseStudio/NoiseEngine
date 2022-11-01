using System;

namespace NoiseEngine.Rendering;

internal static class GraphicsMemoryHelper {

    public static void WaitToCollect() {
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }

}
