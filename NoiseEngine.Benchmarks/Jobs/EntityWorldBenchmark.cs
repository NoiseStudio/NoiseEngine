using BenchmarkDotNet.Attributes;
using NoiseEngine.Jobs;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Benchmarks.Jobs;

[MemoryDiagnoser]
public class EntityWorldBenchmark {

    private readonly EntityWorld world = new EntityWorld();

    public EntityWorldBenchmark() {
        world.NewEntity();
        world.NewEntity(new TestComponentA(), new TestComponentB());
    }

    [Benchmark]
    public void NewEntity() {
        world.NewEntity();
    }

    [Benchmark]
    public void NewEntityWithComponents() {
        world.NewEntity(new TestComponentA(), new TestComponentB());
    }

    [Benchmark]
    public void GetGroupFromComponents() {
        world.GetGroupFromComponents(new List<Type>() {
            typeof(TestComponentA), typeof(TestComponentB)
        });
    }

}
