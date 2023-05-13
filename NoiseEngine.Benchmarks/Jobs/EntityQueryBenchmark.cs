using BenchmarkDotNet.Attributes;
using NoiseEngine.Jobs;

namespace NoiseEngine.Benchmarks.Jobs;

[MemoryDiagnoser]
public class EntityQueryBenchmark {

    private readonly EntityWorld world = new EntityWorld();
    private readonly EntityQuery<TestComponentA, TestComponentB> query;

    [Params(3072)]
    public int EntityCount { get; set; }

    public EntityQueryBenchmark() {
        int a = EntityCount / 3;
        for (int i = 0; i < a; i++) {
            world.Spawn();
            world.Spawn(new TestComponentA());
            world.Spawn(new TestComponentA(), new TestComponentB());
        }

        query = new EntityQuery<TestComponentA, TestComponentB>(world);
    }

    [Benchmark]
    public void Foreach() {
        int count = 0;
        foreach ((Entity, TestComponentA, TestComponentB) element in query)
            count++;
    }

}
