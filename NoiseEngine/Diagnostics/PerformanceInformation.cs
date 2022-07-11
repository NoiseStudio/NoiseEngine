using System.Runtime.InteropServices;

namespace NoiseEngine.Diagnostics;

[StructLayout(LayoutKind.Sequential)]
internal readonly struct PerformanceInformation {

    public int Size { get; }
    public nuint CommitTotal { get; }
    public nuint CommitLimit { get; }
    public nuint CommitPeak { get; }
    public nuint PhysicalTotal { get; }
    public nuint PhysicalAvailable { get; }
    public nuint SystemCache { get; }
    public nuint KernelTotal { get; }
    public nuint KernelPaged { get; }
    public nuint KernelNonPaged { get; }
    public nuint PageSize { get; }
    public int HandlesCount { get; }
    public int ProcessCount { get; }
    public int ThreadCount { get; }

}
