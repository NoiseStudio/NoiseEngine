using Xunit;
using System.Threading;

namespace NoiseStudio.JobsAg.Tests {
    public class JobsQueueTest {

        [Fact]
        public void Test1() {
            JobWorld world = new JobWorld(new uint[] { 5 });
            world.NewJob(Test1, 10);

            Assert.Empty(world.queue.endQueue);
            Thread.Sleep(10);
            Assert.Single(world.queue.endQueue);
        }

    }
}

