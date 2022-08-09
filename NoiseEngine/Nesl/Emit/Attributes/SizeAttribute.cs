using System;
using System.Buffers.Binary;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class SizeAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(SizeAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Type | AttributeTargets.Field;

    public ulong Size => BinaryPrimitives.ReadUInt64LittleEndian(Bytes.AsSpan());

    /// <summary>
    /// Creates new <see cref="SizeAttribute"/>.
    /// </summary>
    /// <param name="size">Size of element.</param>
    /// <returns><see cref="SizeAttribute"/> with given parameters.</returns>
    public static SizeAttribute Create(ulong size) {
        byte[] bytes = new byte[sizeof(ulong)];
        BinaryPrimitives.WriteUInt64LittleEndian(bytes, size);

        return new SizeAttribute {
            FullName = ExpectedFullName,
            Targets = ExpectedTargets,
            Bytes = bytes.ToImmutableArray()
        };
    }

    /// <summary>
    /// Asserts that properties have valid values.
    /// </summary>
    /// <exception cref="InvalidOperationException">Not all properties have valid values.</exception>
    public override void AssertValid() {
        AssertValidFullName(ExpectedFullName);
        AssertValidTargets(ExpectedTargets);
        AssertValidBytesLength(sizeof(ulong));
    }

}
