﻿using NoiseEngine.Collections;
using System;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class PlatformDependentTypeRepresentationAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(PlatformDependentTypeRepresentationAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Type;

    public string? CilTargetName => AttributeHelper.ReadString(Bytes.AsSpan());
    public string? SpirVTargetName => AttributeHelper.ReadString(AttributeHelper.JumpToNextBytes(Bytes.AsSpan()));

    /// <summary>
    /// Creates new <see cref="PlatformDependentTypeRepresentationAttribute"/>.
    /// </summary>
    /// <param name="cilTargetName">Target name in CIL.</param>
    /// <param name="spirVTargetName">Target name in SPIR-V.</param>
    /// <returns><see cref="PlatformDependentTypeRepresentationAttribute"/> with given parameters.</returns>
    public static PlatformDependentTypeRepresentationAttribute Create(string? cilTargetName, string? spirVTargetName) {
        FastList<byte> buffer = new FastList<byte>();

        AttributeHelper.WriteString(buffer, cilTargetName);
        AttributeHelper.WriteString(buffer, spirVTargetName);

        return new PlatformDependentTypeRepresentationAttribute {
            FullName = ExpectedFullName,
            Targets = ExpectedTargets,
            Bytes = buffer.ToImmutableArray()
        };
    }

    /// <summary>
    /// Checks if that properties have valid values.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when attribute properties are valid; otherwise <see langword="false"/>.
    /// </returns>
    public override bool CheckIsValid() {
        return CheckIfValidFullName(ExpectedFullName) && CheckIfValidTargets(ExpectedTargets);
    }

}