using NoiseEngine.Mathematics;
using Xunit;

namespace NoiseEngine.Tests {
    public class ApplicationTest {

        [Fact]
        public void SimpleScene() {
            using Application application = Application.Create();

            for (int i = -10; i < 10; i += 2)
                application.Primitive.CreateCube(new Float3(i));

            application.WaitToEnd();
        }

    }
}
