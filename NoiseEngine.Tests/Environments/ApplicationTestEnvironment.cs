using NoiseEngine.Jobs;
using NoiseEngine.Rendering;
using NoiseEngine.Tests.Fixtures;
using System;

namespace NoiseEngine.Tests.Environments;

[Collection(nameof(ApplicationCollection))]
public abstract class ApplicationTestEnvironment : GraphicsTestEnvironment {

    public EntityWorld EntityWorld => Fixture.EntityWorld;
    public JobsWorld JobsWorld => Fixture.JobsWorld;

    protected ApplicationTestEnvironment(ApplicationFixture fixture) : base(fixture) {
    }

    public void ExecuteOnAllDevices(Action<ApplicationScene> executor) {
        foreach (GraphicsDevice device in GraphicsDevices) {
            using ApplicationScene scene = new ApplicationScene() { GraphicsDevice = device };
            executor(scene);
        }
    }

}
