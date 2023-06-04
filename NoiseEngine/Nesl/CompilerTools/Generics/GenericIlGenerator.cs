using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using NoiseEngine.Nesl.Serialization;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal class GenericIlGenerator {

    private readonly SerializedIlContainer container;
    private readonly NeslType oldType;
    private readonly NeslType newType;
    private readonly NeslMethod genericMethod;
    private readonly IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes;
    private int changes;

    private GenericIlGenerator(
        SerializedIlContainer container, NeslType oldType, NeslType newType, NeslMethod genericMethod,
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        this.container = container;
        this.oldType = oldType;
        this.newType = newType;
        this.genericMethod = genericMethod;
        this.targetTypes = targetTypes;
    }

    public static IlContainer RemoveGenerics(
        NeslType oldType, NeslType newType, NeslMethod genericMethod,
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        if (genericMethod.IsAbstract)
            return genericMethod.GetIlContainer();

        IntrinsicAttribute intrinsic = IntrinsicAttribute.Create();
        if (genericMethod.Attributes.Any(x => x.FullName == intrinsic.FullName))
            return genericMethod.GetIlContainer();

        Debug.Assert(genericMethod.GetInstructions().Last().OpCode == (
            genericMethod.ReturnType is null ? OpCode.Return : OpCode.ReturnValue
        ));

        GenericIlGenerator g = new GenericIlGenerator(
            new SerializedIlContainer(
                genericMethod.Assembly,
                genericMethod.GetIlContainer().GetRawInstructions().ToArray(),
                genericMethod.GetIlContainer().GetTail(0).ToArray()
            ),
            oldType, newType, genericMethod, targetTypes
        );

        int i = 0;
        foreach ((OpCode opCode, uint tailIndex) in g.container.GetRawInstructions()) {
            Instruction instruction = new Instruction(opCode, tailIndex, g.container);
            switch (opCode) {
                case OpCode.DefVariable:
                    g.DefVariable(instruction, tailIndex);
                    break;
                case OpCode.Call:
                    g.Call(i, instruction, tailIndex);
                    break;
            }
            i++;
        }

        return g.changes == 0 ? genericMethod.GetIlContainer() : g.container;
    }

    private void DefVariable(Instruction instruction, uint tailIndex) {
        NeslType typeA = genericMethod.Assembly.GetType(instruction.ReadUInt64());
        NeslType typeB = GenericHelper.GetFinalType(oldType, newType, typeA, targetTypes);
        if (typeA != typeB) {
            BinaryPrimitives.WriteUInt64LittleEndian(
                container.GetWritableTail((int)tailIndex),
                genericMethod.Assembly.GetLocalTypeId(typeB)
            );
            changes++;
        }
    }

    private void Call(int i, Instruction instruction, uint tailIndex) {
        instruction.OffsetTailIndex(4);
        NeslMethod method = genericMethod.Assembly.GetMethod(instruction.ReadUInt64());
        NeslType finalType = GenericHelper.GetFinalType(oldType, newType, method.Type, targetTypes);
        if (method.Type == finalType)
            return;

        NeslMethod finalMethod = finalType.Methods.First(
            x => x.Name == method.Name && x.ParameterTypes.SequenceEqual(method.ParameterTypes.Select(x =>
            GenericHelper.GetFinalType(oldType, newType, x, targetTypes)))
        );

        CallOpCodeAttribute callOpCode = CallOpCodeAttribute.Create(0);
        NeslAttribute? attribute = finalMethod.Attributes.FirstOrDefault(x => x.FullName == callOpCode.FullName);
        if (attribute is not null) {
            callOpCode = attribute.Cast<CallOpCodeAttribute>();
            container.ReplaceOpCode(i, callOpCode.OpCode);

            Span<byte> bytes = stackalloc byte[sizeof(uint) * finalMethod.ParameterTypes.Count];
            container.GetWritableTail((int)tailIndex + sizeof(uint) + sizeof(ulong) + sizeof(uint))
                .Slice(0, bytes.Length).CopyTo(bytes);
            bytes.CopyTo(container.GetWritableTail((int)tailIndex + sizeof(uint)));
        } else {
            BinaryPrimitives.WriteUInt64LittleEndian(
                container.GetWritableTail((int)tailIndex + 4),
                genericMethod.Assembly.GetLocalMethodId(finalMethod)
            );
        }

        changes++;
    }

}
