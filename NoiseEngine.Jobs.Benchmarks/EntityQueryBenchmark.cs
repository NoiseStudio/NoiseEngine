using BenchmarkDotNet.Attributes;

namespace NoiseEngine.Jobs.Benchmarks {
    [MemoryDiagnoser]
    public class EntityQueryBenchmark {

        private readonly EntityWorld world = new EntityWorld();
        private readonly EntityQuery query;
        private readonly EntityQuery<TestComponentA, TestComponentB> queryWithComponents;

        [Params(3072)]
        public int EntityCount { get; set; }

        public EntityQueryBenchmark() {
            int a = EntityCount / 3;
            for (int i = 0; i < a; i++) {
                world.NewEntity();
                world.NewEntity(new TestComponentA());
                world.NewEntity(new TestComponentA(), new TestComponentB());
            }

            query = new EntityQuery(world, false);
            queryWithComponents = new EntityQuery<TestComponentA, TestComponentB>(world, true);
        }

        [Benchmark]
        public void Foreach() {
            int count = 0;
            foreach (Entity entity in query)
                count++;
        }

        [Benchmark]
        public void ForeachWithComponents() {
            int count = 0;
            foreach ((Entity, TestComponentA, TestComponentB) element in queryWithComponents)
                count++;
        }

    }
}
