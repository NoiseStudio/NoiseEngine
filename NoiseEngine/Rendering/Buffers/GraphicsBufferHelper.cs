﻿using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering.Vulkan.Buffers;
using NoiseEngine.Rendering.Exceptions;
using System;

namespace NoiseEngine.Rendering.Buffers;

internal static class GraphicsBufferHelper<T> where T : unmanaged {

    public static InteropHandle<GraphicsReadOnlyBuffer<T>> CreateHandle(
        GraphicsDevice device, GraphicsBufferUsage usage, ulong size, bool map,
        out InteropHandle<GraphicsReadOnlyBuffer<T>> innerHandle
    ) {
        device.Initialize();

        Exception? exception = null;

        int i = 0;
        while (true) {
            // Tries to create a graphic buffer.
            VulkanBufferCreateReturnValue returnValue;
            switch (device.Instance.Api) {
                case GraphicsApi.Vulkan:
                    if (!VulkanBufferInterop.Create(device.Handle, usage, size, map).TryGetValue(
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
                innerHandle = new InteropHandle<GraphicsReadOnlyBuffer<T>>(returnValue.InnerHandle);
                return new InteropHandle<GraphicsReadOnlyBuffer<T>>(returnValue.Handle);
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
