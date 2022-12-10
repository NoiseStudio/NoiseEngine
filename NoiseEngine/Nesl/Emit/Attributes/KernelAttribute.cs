using NoiseEngine.Mathematics;
using System;
using System.Buffers.Binary;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.Emit.Attributes;

public class KernelAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(KernelAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Method;

    public Vector3<uint> LocalSize {
        get {
            ReadOnlySpan<byte> span = Bytes.AsSpan();
            int size = sizeof(uint);

            return new Vector3<uint>(
                BinaryPrimitives.ReadUInt32LittleEndian(span),
                BinaryPrimitives.ReadUInt32LittleEndian(span[size..]),
                BinaryPrimitives.ReadUInt32LittleEndian(span[(size * 2)..])
            );
        }
    }

    /// <summary>
    /// Creates new <see cref="KernelAttribute"/>.
    /// </summary>
    /// <param name="localSize">Local size of this kernel.</param>
    /// <returns><see cref="KernelAttribute"/> with given parameters.</returns>
    public static KernelAttribute Create(Vector3<uint> localSize) {
        int size = sizeof(uint);
        byte[] bytes = new byte[size * 3];
        Span<byte> span = bytes.AsSpan();
        BinaryPrimitives.WriteUInt32LittleEndian(span, localSize.X);
        BinaryPrimitives.WriteUInt32LittleEndian(span[size..], localSize.Y);
        BinaryPrimitives.WriteUInt32LittleEndian(span[(size * 2)..], localSize.Z);

        return new KernelAttribute {
            FullName = ExpectedFullName,
            Targets = ExpectedTargets,
            Bytes = bytes.ToImmutableArray()
        };
    }

    /// <summary>
    /// Checks if that properties have valid values.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when attribute properties are valid; otherwise <see langword="false"/>.
    /// </returns>
    public override bool CheckIsValid() {
        return CheckIfValidFullName(ExpectedFullName) &&
            CheckIfValidTargets(ExpectedTargets) &&
            CheckIfValidBytesLength(sizeof(uint) * 3);
    }

}
