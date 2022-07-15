using BenchmarkDotNet.Attributes;
using NoiseEngine.Jobs;

namespace NoiseEngine.Benchmarks.Jobs;

[MemoryDiagnoser]
public class JobsWorldBenchmark {

    private readonly JobsInvoker invoker;
    private readonly JobsWorld world;

    public JobsWorldBenchmark() {
        invoker = new JobsInvoker();
        world = new JobsWorld(invoker, new uint[] {
            2, 3, 5, 10
        });
    }

    [Benchmark]
    public void Enqueue() {
        world.EnqueueJob(TestMethodEnqueuing, 0);
    }

    private void TestMethodEnqueuing() {
    }

}
