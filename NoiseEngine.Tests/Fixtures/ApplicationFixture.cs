using System;
using NoiseEngine.Logging;

namespace NoiseEngine.Tests.Fixtures;

public class ApplicationFixture : IDisposable {

    public ApplicationFixture() {
        Application.Initialize(new ApplicationSettings {
            ProcessExitOnApplicationExit = false
        });

        Log.Logger.LogLevelMask = LogLevel.All;
    }

    public void Dispose() {
        Application.Exit();
    }

}
