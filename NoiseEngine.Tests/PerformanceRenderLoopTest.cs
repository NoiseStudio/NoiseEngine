using NoiseEngine.Rendering;
using NoiseEngine.Tests.Environments;
using NoiseEngine.Tests.Fixtures;
using System;
using System.Threading;

namespace NoiseEngine.Tests;

public class PerformanceRenderLoopTest : ApplicationTestEnvironment {

    private const int ResizeCount = 32;
    private const int WaitTime = 8;
    private const int MinSize = 16;
    private const int MaxSize = 512;

    public PerformanceRenderLoopTest(ApplicationFixture fixture) : base(fixture) {
    }

    [FactRequire(TestRequirements.Gui | TestRequirements.Graphics)]
    public void EqualFramesInFlightAndExecutors() {
        Helper(nameof(LessFramesInFlightThanExecutors), 5, null);
    }

    [FactRequire(TestRequirements.Gui | TestRequirements.Graphics)]
    public void MoreFramesInFlightThanExecutors() {
        Helper(nameof(LessFramesInFlightThanExecutors), 5, 1);
    }

    [FactRequire(TestRequirements.Gui | TestRequirements.Graphics)]
    public void LessFramesInFlightThanExecutors() {
        Helper(nameof(LessFramesInFlightThanExecutors), 1, 5);
    }

    private void Helper(string title, uint framesInFlight, uint? executionThreadCount) {
        ExecuteOnAllDevices(scene => {
            Window window = Fixture.GetWindow(title);
            Camera camera = new Camera(scene) {
                RenderTarget = window,
                RenderLoop = new PerformanceRenderLoop() {
                    FramesInFlight = framesInFlight,
                    ExecutionThreadCount = executionThreadCount
                },
                ClearColor = Color.Random
            };

            for (int i = 0; i < ResizeCount; i++) {
                window.Resize(GetRandomSize(), GetRandomSize());
                Thread.Sleep(WaitTime);
            }
        });
    }

    private uint GetRandomSize() {
        return (uint)Random.Shared.Next(MinSize, MaxSize);
    }

}
