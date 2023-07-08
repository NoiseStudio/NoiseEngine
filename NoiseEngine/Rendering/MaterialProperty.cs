using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering;

public abstract class MaterialProperty {

    public CommonMaterial Material { get; }
    public MaterialPropertyType Type { get; }
    public string Name { get; }
    public object? Value { get; private set; }

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
    /// <param name="texture">
    /// <see cref="Texture2D"/> which will be assigned to this <see cref="MaterialProperty"/>.
    /// </param>
    public void SetTexture(Texture2D texture) {
        AssertType(MaterialPropertyType.Texture2D);
        Value = texture;
        SetTexture2DUnchecked(texture);
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

    private protected abstract void SetTexture2DUnchecked(Texture2D texture);
    private protected abstract void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) where T : unmanaged;

    private void AssertType(MaterialPropertyType usedType) {
        if (Type != usedType)
            throw new InvalidOperationException($"This {nameof(MaterialProperty)} is of type {Type}, not {usedType}.");
    }

}
