using Xunit;

namespace NoiseStudio.JobsAg.Tests {
    public class JobWorldTest {

        [Fact]
        public void NewJob() {
            JobWorld world = new JobWorld();
            world.NewJob(Method, 5, 5);
        }

        private void Method(int abc) {

        }

    }
}
