using NoiseEngine.Nesl;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;
using System;
using System.Collections.Generic;

namespace NoiseEngine.Rendering;

public abstract class CommonMaterial {

    private readonly Dictionary<NeslField, MaterialProperty>? properties;

    internal CommonMaterialDelegation? Delegation { get; }

    private protected CommonMaterial(ICommonShader shader) {
        if (shader.Delegation.Properties is not null) {
            properties = new Dictionary<NeslField, MaterialProperty>();
            foreach ((NeslField field, MaterialProperty property) in shader.Delegation.Properties)
                properties.Add(field, property.Clone(this));

            Delegation = shader.Device.Instance.Api switch {
                GraphicsApi.Vulkan => new VulkanCommonMaterialDelegation(shader, properties),
                _ => throw new GraphicsApiNotSupportedException(shader.Device.Instance.Api)
            };
        }
    }

    /// <summary>
    /// Tries return <see cref="MaterialProperty"/> created from given <paramref name="neslProperty"/>.
    /// </summary>
    /// <param name="neslProperty">Origin <see cref="NeslField"/> of returned <see cref="MaterialProperty"/>.</param>
    /// <returns>
    /// <see cref="MaterialProperty"/> when this <see cref="ComputeShader"/> contains given
    /// <paramref name="neslProperty"/>; otherwise null.
    /// </returns>
    public MaterialProperty? GetProperty(NeslField neslProperty) {
        if (properties is null)
            return null;
        return properties.TryGetValue(neslProperty, out MaterialProperty? property) ? property : null;
    }

}
