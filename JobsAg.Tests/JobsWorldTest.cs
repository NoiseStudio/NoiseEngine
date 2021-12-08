using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class JobsWorldTest {

        [Fact]
        public void CreateWithoutQueues() {
            JobsWorld world = new JobsWorld(new uint[0]);
            world.NewJob(TestMethodT0, 0);

            Assert.Single(world.queue.endQueue);
        }

        [Fact]
        public void CreateWithQueues() {
            JobsWorld world = new JobsWorld(new uint[] { 1 });
            world.NewJob(TestMethodT0, 10);

            Assert.Empty(world.queue.endQueue);
        }

        [Fact]
        public void NewJobT0() {
            JobsWorld world = new JobsWorld();
            world.NewJob(TestMethodT0, 0);
        }

        [Fact]
        public void NewJobT1() {
            JobsWorld world = new JobsWorld();
            world.NewJob(TestMethodT1, 0, "Hello");
        }

        private void TestMethodT0() {
        }

        private void TestMethodT1(string a) {
        }

    }
}
