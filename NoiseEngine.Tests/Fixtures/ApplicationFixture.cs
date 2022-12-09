using NoiseEngine.Logging;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Tests.Fixtures;

public class ApplicationFixture : IDisposable {

    internal IReadOnlyList<GraphicsDevice> GraphicsDevices { get; }
    internal IReadOnlyList<VulkanDevice> VulkanDevices { get; }

    public ApplicationFixture() {
        Application.Initialize(new ApplicationSettings {
            ProcessExitOnApplicationExit = false
        });

        Log.Logger.LogLevelMask = LogLevel.All;

        if (
            TestRequirements.Graphics.ToSkipReason().Length == 0 && TestRequirements.Vulkan.ToSkipReason().Length == 0
        ) {
            GraphicsDevices = Application.GraphicsInstance.Devices;
        } else {
            GraphicsDevices = new GraphicsDevice[0];
        }

        VulkanDevices = GraphicsDevices.Where(x => x.Instance.Api == GraphicsApi.Vulkan).Cast<VulkanDevice>().ToArray();
    }

    public void Dispose() {
        Application.Exit();
    }

}
