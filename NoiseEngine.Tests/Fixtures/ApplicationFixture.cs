using System;
using System.Collections.Generic;
using System.Linq;
using NoiseEngine.Logging;
using NoiseEngine.Rendering;

namespace NoiseEngine.Tests.Fixtures;

public class ApplicationFixture : IDisposable {

    public IReadOnlyList<GraphicsDevice> GraphicsDevices { get; }

    public ApplicationFixture() {
        Application.Initialize(new ApplicationSettings {
            ProcessExitOnApplicationExit = false
        });

        Log.Logger.LogLevelMask = LogLevel.All;

        GraphicsDevices = Application.GraphicsInstance.Devices.Where(x => !x.Name.StartsWith("llvm")).ToArray();

        Log.Info($"Used {nameof(GraphicsDevice)} for tests:");
        int i = 0;
        foreach (GraphicsDevice device in GraphicsDevices)
            Log.Info($"{i++} - {device}");
    }

    public void Dispose() {
        Application.Exit();
    }

}
