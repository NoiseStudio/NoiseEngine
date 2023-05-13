using BenchmarkDotNet.Attributes;
using NoiseEngine.Jobs;

namespace NoiseEngine.Benchmarks.Jobs;

[MemoryDiagnoser]
public class EntitySystemBenchmark {

    private readonly EntityWorld world = new EntityWorld();
    private readonly TestSystemA system;

    [Params(1024)]
    public int EntityCount { get; set; }

    public EntitySystemBenchmark() {
        for (int i = 0; i < EntityCount; i++)
            world.Spawn();

        system = new TestSystemA();
        world.AddSystem(system);
    }

    [Benchmark]
    public void ExecuteMultithread() {
        system.ExecuteAndWait();
    }

}
