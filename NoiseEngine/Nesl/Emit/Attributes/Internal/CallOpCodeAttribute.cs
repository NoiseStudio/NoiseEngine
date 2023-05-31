using System;
using System.Buffers.Binary;
using System.Collections.Immutable;

namespace NoiseEngine.Nesl.Emit.Attributes.Internal;

internal class CallOpCodeAttribute : NeslAttribute {

    private const string ExpectedFullName = nameof(CallOpCodeAttribute);
    private const AttributeTargets ExpectedTargets = AttributeTargets.Method;

    public OpCode OpCode => (OpCode)BinaryPrimitives.ReadUInt16LittleEndian(Bytes.AsSpan());

    /// <summary>
    /// Creates new <see cref="CallOpCodeAttribute"/>.
    /// </summary>
    /// <param name="opCode"><see cref="OpCode"/> called instead of method.</param>
    /// <returns><see cref="CallOpCodeAttribute"/> with given parameters.</returns>
    public static CallOpCodeAttribute Create(ushort opCode) {
        Span<byte> span = stackalloc byte[sizeof(ushort)];
        BinaryPrimitives.WriteUInt16LittleEndian(span, opCode);

        return new CallOpCodeAttribute {
            FullName = ExpectedFullName,
            Targets = ExpectedTargets,
            Bytes = span.ToImmutableArray()
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
            CheckIfValidBytesLength(sizeof(ushort));
    }

}
