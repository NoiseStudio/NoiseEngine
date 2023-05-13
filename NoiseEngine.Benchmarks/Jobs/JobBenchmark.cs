using BenchmarkDotNet.Attributes;
using NoiseEngine.Jobs;

namespace NoiseEngine.Benchmarks.Jobs;

[MemoryDiagnoser]
public class JobBenchmark {

    private readonly JobsInvoker invoker;
    private readonly JobsWorld world;
    private readonly Job job;

    public JobBenchmark() {
        invoker = new JobsInvoker();
        world = new JobsWorld(invoker);
        job = new JobEmpty(world, 0, TestMethodEnqueuing);
    }

    [Benchmark]
    public void TryInvoke() {
        job.TryInvoke();
    }

    private void TestMethodEnqueuing() {
    }

}
