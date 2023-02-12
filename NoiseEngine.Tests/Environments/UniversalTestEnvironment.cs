using System;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Environments;

public abstract class UniversalTestEnvironment {

    public void RunParallel(Action action, int quantityPerThread = 64) {
        Task[] tasks = new Task[Environment.ProcessorCount * 4];
        for (int i = 0; i < tasks.Length; i++) {
            tasks[i] = Task.Run(() => {
                for (int i = 0; i < quantityPerThread; i++)
                    action();
            });
        }
        Task.WaitAll(tasks);
    }

}
