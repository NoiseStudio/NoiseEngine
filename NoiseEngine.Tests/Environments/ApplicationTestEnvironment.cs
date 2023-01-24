using NoiseEngine.Rendering;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Environments;

[Collection(nameof(ApplicationCollection))]
public abstract class ApplicationTestEnvironment : GraphicsTestEnvironment {

    protected ApplicationTestEnvironment(ApplicationFixture fixture) : base(fixture) {
    }

    public void ExecuteOnAllDevices(Action<ApplicationScene> executor) {
        foreach (GraphicsDevice device in GraphicsDevices) {
            using ApplicationScene scene = new ApplicationScene() { GraphicsDevice = device };
            executor(scene);
        }
    }

}
