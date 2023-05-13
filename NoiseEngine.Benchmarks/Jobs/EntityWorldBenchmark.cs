using BenchmarkDotNet.Attributes;
using NoiseEngine.Jobs;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Benchmarks.Jobs;

[MemoryDiagnoser]
public class EntityWorldBenchmark {

    private readonly EntityWorld world = new EntityWorld();

    public EntityWorldBenchmark() {
        world.Spawn();
        world.Spawn(new TestComponentA(), new TestComponentB());
    }

    [Benchmark]
    public void NewEntity() {
        world.Spawn();
    }

    [Benchmark]
    public void NewEntityWithComponents() {
        world.Spawn(new TestComponentA(), new TestComponentB());
    }

}
