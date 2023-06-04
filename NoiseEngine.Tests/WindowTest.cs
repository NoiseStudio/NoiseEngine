using System.Threading;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests;

public class WindowTest : ApplicationTestEnvironment {

    private const int SetTitleCount = 32;
    
    private const int WaitTime = 8;
    
    public WindowTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Gui)]
    public void CreateAndDispose() {
        using Window window = new Window();
    }
    
    [FactRequire(TestRequirements.Gui)]
    public void SetTitle() {
        using Window window = new Window();
        
        for (int i = 0; i < SetTitleCount; i++) {
            window.SetTitle("counter: " + i);
            Thread.Sleep(WaitTime);
        }
    }
    
}
