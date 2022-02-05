using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NoiseEngine.Threading.Tests {
    public class SingleThreadInvokerTest {

        [Theory]
        [InlineData(10)]
        public async Task ExecuteMultiple(int count) {
            SingleThreadInvoker invoker = new SingleThreadInvoker();

            int executes = 0;
            Thread? runningThread = null;

            for (int i = 0; i < count; i++) {
                invoker.Execute(() => {
                    executes++;

                    if (runningThread == null)
                        runningThread = Thread.CurrentThread;
                    else
                        Assert.Equal(runningThread, Thread.CurrentThread);
                });
            }

            await Task.Delay(1000);
            Assert.Equal(count, executes);
            invoker.Destroy();
        }

        [Fact]
        public void ExecuteAndWait() {
            SingleThreadInvoker invoker = new SingleThreadInvoker();
            bool executed = false;
            invoker.ExecuteAndWait(() => executed = true);
            Assert.True(executed);
            invoker.Destroy();
        }

        [Theory]
        [InlineData(10)]
        public void ExecuteMultipleAsync(int count) {
            SingleThreadInvoker invoker = new SingleThreadInvoker();

            Task[] tasks = new Task[count];

            int executes = 0;
            Thread? runningThread = null;

            for (int i = 0; i < count; i++) {
                tasks[i] = Task.Run(() => invoker.Execute(() => {
                    executes++;

                    if (runningThread == null)
                        runningThread = Thread.CurrentThread;
                    else
                        Assert.Equal(runningThread, Thread.CurrentThread);
                }));
            }

            Task.WaitAll(tasks);
            Task.Delay(1000).Wait();
            Assert.Equal(count, executes);

            invoker.Destroy();
        }
    }
}
