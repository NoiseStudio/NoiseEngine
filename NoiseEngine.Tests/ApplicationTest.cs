using NoiseEngine.DeveloperTools.Components;
using NoiseEngine.DeveloperTools.Systems;
using NoiseEngine.Jobs;
using NoiseEngine.Mathematics;
using Xunit;

namespace NoiseEngine.Tests {
    public class ApplicationTest {

        [Fact]
        public void SimpleScene() {
            using Application application = Application.Create(out Entity cameraEntity);

            for (int x = -10; x < 10; x += 2) {
                for (int y = -10; y < 10; y += 2) {
                    application.Primitive.CreateCube(new Float3(x, 0, y));
                }
            }

            cameraEntity.Add(application.World, new DebugMovementComponent());
            application.AddFrameDependentSystem(new DebugMovementSystem(application));

            application.WaitToEnd();
        }

    }
}
