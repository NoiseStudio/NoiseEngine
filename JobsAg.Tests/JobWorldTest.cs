using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class JobWorldTest {

        [Fact]
        public void CreateWithoutQueues() {
            JobWorld world = new JobWorld(new uint[0]);
            world.NewJob(TestMethodT0, 0);

            Assert.Single(world.queue.endQueue);
        }

        [Fact]
        public void CreateWithQueues() {
            JobWorld world = new JobWorld(new uint[] { 1 });
            world.NewJob(TestMethodT0, 10);

            Assert.Empty(world.queue.endQueue);
        }

        [Fact]
        public void NewJobT0() {
            JobWorld world = new JobWorld();
            world.NewJob(TestMethodT0, 0);
        }

        [Fact]
        public void NewJobT1() {
            JobWorld world = new JobWorld();
            world.NewJob(TestMethodT1, 0, "Hello");
        }

        private void TestMethodT0() {
        }

        private void TestMethodT1(string a) {
        }

    }
}
