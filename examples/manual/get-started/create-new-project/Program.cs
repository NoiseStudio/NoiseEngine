using NoiseEngine;
using NoiseEngine.DeveloperTools.Systems;

Application.Initialize(new());

var scene = new ApplicationScene();
var camera = new Camera(scene) {
    RenderTarget = new Window(),
    RenderLoop = new PerformanceRenderLoop()
};
DebugMovementSystem.InitializeTo(camera);

scene.Primitive.CreateCube();

Application.WaitToEnd();
