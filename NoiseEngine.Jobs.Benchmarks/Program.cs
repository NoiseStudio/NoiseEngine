using BenchmarkDotNet.Running;

namespace NoiseEngine.Jobs.Benchmarks {
    internal class Program {
        internal static void Main(string[] args) {
            BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
