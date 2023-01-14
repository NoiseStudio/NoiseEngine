using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests;

public class WindowTest : ApplicationTestEnvironment {

    public WindowTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Gui)]
    public void CreateAndDispose() {
        using Window window = new Window();
    }

}
