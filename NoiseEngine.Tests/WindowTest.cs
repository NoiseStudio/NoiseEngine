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
        Window window = Fixture.GetWindow("counter: null");
        
        for (int i = 0; i < SetTitleCount; i++) {
            window.Title = "counter: " + i;
            Thread.Sleep(WaitTime);
        }
    }
    
}
