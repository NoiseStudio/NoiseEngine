using System;

namespace NoiseEngine.Tests.Fixtures;

public class ApplicationFixture : IDisposable {

    public ApplicationFixture() {
        Application.Initialize(new ApplicationSettings {
            ProcessExitOnApplicationExit = false
        });
    }

    public void Dispose() {
        Application.Exit();
    }

}
