using NoiseEngine.Diagnostics;

namespace NoiseEngine.Tests.Diagnostics;

public class MemoryInfoTest {

    [Fact]
    public void Usage() {
        Assert.NotEqual(0u, MemoryInfo.TotalPhysicalMemory);
        Assert.NotEqual(0u, MemoryInfo.AvailablePhysicalMemory);

        Assert.True(MemoryInfo.TotalPhysicalMemory > MemoryInfo.AvailablePhysicalMemory);
    }

}
