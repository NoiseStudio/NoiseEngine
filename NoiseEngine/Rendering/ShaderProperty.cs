using NoiseEngine.Rendering.Buffers;
using System;

namespace NoiseEngine.Rendering;

public abstract class ShaderProperty {

    public ShaderPropertyType Type { get; }
    public string Name { get; }
    public object? Value { get; private set; }

    internal CommonShaderDelegation ShaderDelegation { get; }
    internal int Index { get; }

    private protected ShaderProperty(
        CommonShaderDelegation shaderDelegation, int index, ShaderPropertyType type, string name
    ) {
        ShaderDelegation = shaderDelegation;
        Index = index;
        Type = type;
        Name = name;
    }

    public void SetBuffer<T>(GraphicsBuffer<T> buffer) where T : unmanaged {
        AssertType(ShaderPropertyType.Buffer);
        Value = buffer;
        SetBufferUnchecked(buffer);
    }

    internal abstract ShaderProperty Clone(CommonShaderDelegation newShaderDelegation);

    private protected abstract void SetBufferUnchecked<T>(GraphicsBuffer<T> buffer) where T : unmanaged;

    private void AssertType(ShaderPropertyType usedType) {
        if (Type != usedType)
            throw new InvalidOperationException($"This {nameof(ShaderProperty)} is of type {Type}, not {usedType}.");
    }

}
