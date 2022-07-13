using NoiseEngine.Threading;
using System.Threading;
using System.Threading.Tasks;

namespace NoiseEngine.Tests.Threading;

public class SingleThreadInvokerTest {

    [Theory]
    [InlineData(10)]
    public void ExecuteMultiple(int count) {
        using SingleThreadInvoker invoker = new SingleThreadInvoker();
        using AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        int executes = 0;
        Thread? runningThread = null;

        for (int i = 0; i < count; i++) {
            invoker.Execute(() => {
                if (runningThread == null)
                    runningThread = Thread.CurrentThread;
                else
                    Assert.Equal(runningThread, Thread.CurrentThread);

                if (count == ++executes)
                    autoResetEvent.Set();
            });
        }

        autoResetEvent.WaitOne();
        Assert.Equal(count, executes);
    }

    [Fact]
    public void ExecuteAndWait() {
        using SingleThreadInvoker invoker = new SingleThreadInvoker();
        bool executed = false;
        invoker.ExecuteAndWait(() => executed = true);
        Assert.True(executed);
    }

    [Theory]
    [InlineData(10)]
    public void ExecuteMultipleAsync(int count) {
        using SingleThreadInvoker invoker = new SingleThreadInvoker();

        Task[] tasks = new Task[count];

        int executes = 0;
        Thread? runningThread = null;

        for (int i = 0; i < count; i++) {
            tasks[i] = Task.Run(() => invoker.ExecuteAndWait(() => {
                executes++;

                if (runningThread == null)
                    runningThread = Thread.CurrentThread;
                else
                    Assert.Equal(runningThread, Thread.CurrentThread);
            }));
        }

        Task.WaitAll(tasks);
        Assert.Equal(count, executes);
    }

}
