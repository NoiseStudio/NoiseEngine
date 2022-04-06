using BenchmarkDotNet.Attributes;

namespace NoiseEngine.Jobs.Benchmarks {
    [MemoryDiagnoser]
    public class EntitySystemBenchmark {

        private readonly EntityWorld world = new EntityWorld();
        private readonly EntitySchedule schedule;
        private readonly TestSystemA system;

        [Params(1024)]
        public int EntityCount { get; set; }

        public EntitySystemBenchmark() {
            for (int i = 0; i < EntityCount; i++)
                world.NewEntity();

            schedule = new EntitySchedule();
            system = new TestSystemA();
            world.AddSystem(system, schedule, null);
        }

        [Benchmark]
        public void ExecuteMultithread() {
            system.ExecuteAndWaitMultithread();
        }

    }
}
