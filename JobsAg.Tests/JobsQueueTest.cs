using Xunit;
using System.Threading;

namespace NoiseStudio.JobsAg.Tests {
    public class JobsQueueTest {

        [Fact]
        public void Test1() {
            JobsWorld world = new JobsWorld(new uint[] { 5 });
            world.NewJob(Test1, 10);

            Assert.Empty(world.queue.endQueue);
            Thread.Sleep(10);
            Assert.Single(world.queue.endQueue);
        }

        [Fact]
        public void Test2() {
            JobsWorld world = new JobsWorld(new uint[] { 5, 15 });
            world.NewJob(Test1, 30);

            Assert.Empty(world.queue.endQueue);
            Thread.Sleep(30);
            Assert.Single(world.queue.endQueue);
        }

    }
}

