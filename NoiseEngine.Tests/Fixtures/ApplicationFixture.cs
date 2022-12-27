using NoiseEngine.Logging;
using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Buffers;
using NoiseEngine.Rendering.Vulkan;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Tests.Fixtures;

public class ApplicationFixture : IDisposable {

    private readonly ConcurrentDictionary<GraphicsDevice, GraphicsCommandBuffer> commandBuffers =
        new ConcurrentDictionary<GraphicsDevice, GraphicsCommandBuffer>();

    internal IReadOnlyList<GraphicsDevice> GraphicsDevices { get; private set; }
    internal IReadOnlyList<VulkanDevice> VulkanDevices { get; private set; }

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
        commandBuffers.Clear();
        GraphicsDevices = Array.Empty<GraphicsDevice>();
        VulkanDevices = Array.Empty<VulkanDevice>();

        Application.Exit();
    }

    internal GraphicsCommandBuffer GetCommandBuffer(GraphicsDevice device) {
        GraphicsCommandBuffer commandBuffer =
            commandBuffers.GetOrAdd(device, static device => new GraphicsCommandBuffer(device, true));

        commandBuffer.Clear();
        return commandBuffer;
    }

}
