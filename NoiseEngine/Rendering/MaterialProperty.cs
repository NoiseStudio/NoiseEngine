using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering;

public abstract class MaterialProperty {

    public CommonMaterial Material { get; }
    public MaterialPropertyType Type { get; }
    public string Name { get; }
    public object? Value { get; private set; }

    public GraphicsDevice Device => Material.Device;

    internal int Index { get; }

    private protected MaterialProperty(
        CommonMaterial material, int index, MaterialPropertyType type, string name
    ) {
        Material = material;
        Index = index;
        Type = type;
        Name = name;
    }

    /// <summary>
    /// Sets <paramref name="texture"/> to this <see cref="MaterialProperty"/>.
    /// </summary>
    /// <remarks>Sets <see cref="Value"/> as <see cref="SampledTexture"/>.</remarks>
    /// <param name="texture">
    /// <see cref="Texture2D"/> which will be assigned to this <see cref="MaterialProperty"/>.
    /// </param>
    /// <param name="sampler">
    /// <see cref="TextureSampler"/> or <see langword="null"/>. When <see langword="null"/>
    /// <see cref="GraphicsDevice.DefaultTextureSampler"/> is used.
    /// </param>
    public void SetTexture(Texture2D texture, TextureSampler? sampler = null) {
        if (!texture.Usage.HasFlag(TextureUsage.Sampled))
            throw new InvalidOperationException("Texture has not TextureUsage.Sampled flag.");

        AssertType(MaterialPropertyType.Texture2D);
        SampledTexture sampled = new SampledTexture(texture, sampler ?? Device.DefaultTextureSampler);
        Value = sampled;
        SetTexture2DUnchecked(sampled);
    }

    /// <summary>
    /// Sets <paramref name="buffer"/> to this <see cref="MaterialProperty"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <paramref name="buffer"/>'s element.</typeparam>
    /// <param name="buffer">
    /// <see cref="GraphicsBuffer{T}"/> which will be assigned to this <see cref="MaterialProperty"/>.
    /// </param>
    public void SetBuffer<T>(GraphicsBuffer<T> buffer) where T : unmanaged {
        AssertType(MaterialPropertyType.Buffer);
        Value = buffer;
        SetBufferUnchecked(buffer);
    }

    internal abstract MaterialProperty Clone(CommonMaterial newMaterial);

    private protected abstract void SetTexture2DUnchecked(SampledTexture sampled);
    private protected abstract void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) where T : unmanaged;

    private void AssertType(MaterialPropertyType usedType) {
        if (Type != usedType)
            throw new InvalidOperationException($"This {nameof(MaterialProperty)} is of type {Type}, not {usedType}.");
    }

}
