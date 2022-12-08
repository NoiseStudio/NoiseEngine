using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class ShaderModuleInterop {

    [InteropImport("rendering_vulkan_shader_module_create")]
    public static partial InteropResult<InteropHandle<ShaderModule>> Create(
        InteropHandle<GraphicsDevice> device, ReadOnlySpan<byte> code
    );

    [InteropImport("rendering_vulkan_shader_module_destroy")]
    public static partial void Destroy(InteropHandle<ShaderModule> handle);

}
