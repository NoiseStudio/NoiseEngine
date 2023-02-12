using NoiseEngine.Jobs2;
using NoiseEngine.Tests.Fixtures;

namespace NoiseEngine.Tests.Environments;

[Collection(nameof(JobsCollection))]
public abstract class JobsTestEnvironment : UniversalTestEnvironment {

    protected JobsFixture Fixture { get; }

    protected EntityWorld EntityWorld => Fixture.EntityWorld;

    protected JobsTestEnvironment(JobsFixture fixture) {
        Fixture = fixture;
    }

}
