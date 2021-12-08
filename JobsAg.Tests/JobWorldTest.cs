using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class JobWorldTest {

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
