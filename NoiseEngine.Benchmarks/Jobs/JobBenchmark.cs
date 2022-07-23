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
        job = new Job(0, TestMethodEnqueuing, JobTime.Zero);
    }

    [Benchmark]
    public void Invoke() {
        job.Invoke(world);
    }

    private void TestMethodEnqueuing() {
    }

}
