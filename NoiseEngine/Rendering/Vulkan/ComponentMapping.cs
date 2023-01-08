namespace NoiseEngine.Rendering.Vulkan;

internal readonly record struct ComponentMapping(
    ComponentSwizzle R,
    ComponentSwizzle G,
    ComponentSwizzle B,
    ComponentSwizzle A
);
