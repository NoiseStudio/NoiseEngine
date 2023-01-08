using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Interop.Rendering.Vulkan;
using NoiseEngine.Mathematics;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering;

internal static class TextureHelper {

    public static InteropHandle<Texture> CreateHandle(
        GraphicsDevice device, TextureCreateInfo createInfo, out InteropHandle<Texture> innerHandle
    ) {
        // Validation.
        const string Message = " of texture can not be zero.";

        if (createInfo.Size.X == 0)
            throw new InvalidOperationException("Width" + Message);
        if (createInfo.Size.Y == 0)
            throw new InvalidOperationException("Height" + Message);
        if (createInfo.Size.Z == 0)
            throw new InvalidOperationException("Depth" + Message);

        if (createInfo.MipLevels == 0)
            throw new InvalidOperationException("Mip levels" + Message);
        if (createInfo.ArrayLayers == 0)
            throw new InvalidOperationException("Array levels" + Message);
        if (createInfo.SampleCount == 0)
            throw new InvalidOperationException("Sample count" + Message);

        // Creation.
        device.Initialize();

        Exception? exception = null;

        int i = 0;
        while (true) {
            // Tries to create a texture.
            TextureCreateReturnValue returnValue;
            switch (device.Instance.Api) {
                case GraphicsApi.Vulkan:
                    if (!VulkanImageInterop.Create(device.Handle, new VulkanImageCreateInfoRaw(createInfo)).TryGetValue(
                        out returnValue, out ResultError error
                    )) {
                        exception = error.ToException();
                        error.Dispose();
                    }
                    break;
                default:
                    throw new GraphicsApiNotSupportedException(device.Instance.Api);
            }

            if (exception is null) {
                innerHandle = returnValue.InnerHandle;
                return returnValue.Handle;
            }

            // The first occurrence of GraphicsOutOfMemoryException is ignored, after which memory cleanup is called
            // and then the graphics buffer creating is tried again. Next occurrences will throw exception.
            if (i++ != 0)
                break;
            if (exception is not GraphicsOutOfMemoryException)
                throw exception;

            GraphicsMemoryHelper.WaitToCollect();
        }

        throw exception;
    }

}
