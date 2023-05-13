using NoiseEngine.Jobs;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Jobs;

public class EntityWorldTest : ApplicationTestEnvironment {

    public EntityWorldTest(ApplicationFixture fixture) : base(fixture) {
    }

    [Fact]
    public void Spawn() {
        Parallel.For(0, 100000, _ => EntityWorld.Spawn(MockComponentA.TestValueA).Despawn());
    }

    [Fact]
    public void SpawnWithRepeats() {
        Assert.Throws<ArgumentException>(() => EntityWorld.Spawn(
           MockComponentA.TestValueA, MockComponentA.TestValueB, MockComponentB.TestValueA
        ));
        Assert.Throws<ArgumentException>(() => EntityWorld.Spawn(
           MockComponentA.TestValueA, MockComponentA.TestValueB, MockComponentA.TestValueB, MockComponentB.TestValueA
        ));
    }

}
