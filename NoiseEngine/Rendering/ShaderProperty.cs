using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering;

public abstract class ShaderProperty {

    public ShaderPropertyType Type { get; }
    public string Name { get; }
    public object? Value { get; private set; }

    internal CommonShaderDelegationOld ShaderDelegation { get; }
    internal int Index { get; }

    private protected ShaderProperty(
        CommonShaderDelegationOld shaderDelegation, int index, ShaderPropertyType type, string name
    ) {
        ShaderDelegation = shaderDelegation;
        Index = index;
        Type = type;
        Name = name;
    }

    /// <summary>
    /// Sets <paramref name="buffer"/> to this <see cref="ShaderProperty"/>.
    /// </summary>
    /// <typeparam name="T">Type of the <paramref name="buffer"/>'s element.</typeparam>
    /// <param name="buffer">
    /// <see cref="GraphicsBuffer{T}"/> which will be assigned to this <see cref="ShaderProperty"/>.
    /// </param>
    public void SetBuffer<T>(GraphicsBuffer<T> buffer) where T : unmanaged {
        AssertType(ShaderPropertyType.Buffer);
        Value = buffer;
        SetBufferUnchecked(buffer);
    }

    internal abstract ShaderProperty Clone(CommonShaderDelegationOld newShaderDelegation);

    private protected abstract void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) where T : unmanaged;

    private void AssertType(ShaderPropertyType usedType) {
        if (Type != usedType)
            throw new InvalidOperationException($"This {nameof(ShaderProperty)} is of type {Type}, not {usedType}.");
    }

}
