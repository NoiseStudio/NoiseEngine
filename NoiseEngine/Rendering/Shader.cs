﻿using NoiseEngine.Nesl;
using NoiseEngine.Rendering.Exceptions;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering;

public class Shader : ICommonShader {

    public GraphicsDevice Device { get; }
    public NeslType ClassData { get; }

    public int Priority { get; init; }

    internal CommonShaderDelegation Delegation { get; }

    ShaderType ICommonShader.Type => ShaderType.VertexFragment;
    CommonShaderDelegation ICommonShader.Delegation => Delegation;

    public Shader(GraphicsDevice device, NeslType classData, ShaderSettings settings) {
        if (classData is null)
            throw new ArgumentNullException(nameof(classData));

        device.Initialize();

        Device = device;
        ClassData = classData;

        Delegation = device.Instance.Api switch {
            GraphicsApi.Vulkan => new VulkanVertexFragmentShaderDelegation(this, settings),
            _ => throw new GraphicsApiNotSupportedException(device.Instance.Api),
        };
    }

    public Shader(GraphicsDevice device, NeslType classData) : this(
        device, classData, ShaderSettings.Performance
    ) {
    }

}
