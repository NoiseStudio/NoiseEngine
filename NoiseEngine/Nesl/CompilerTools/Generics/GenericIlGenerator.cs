using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Serialization;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal static class GenericIlGenerator {

    public static IlContainer RemoveGenerics(
        NeslType oldType, NeslType newType, NeslMethod genericMethod,
        IReadOnlyDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        SerializedIlContainer container = new SerializedIlContainer(
            genericMethod.Assembly,
            genericMethod.GetIlContainer().GetRawInstructions(),
            genericMethod.GetIlContainer().GetTail(0).ToArray()
        );

        NeslType typeA;
        NeslType typeB;

        int changes = 0;
        foreach ((OpCode opCode, uint tailIndex) in container.GetRawInstructions()) {
            Instruction instruction = new Instruction(opCode, tailIndex, container);
            switch (opCode) {
                case OpCode.DefVariable:
                    typeA = genericMethod.Assembly.GetType(instruction.ReadUInt64());
                    typeB = GenericHelper.GetFinalType(oldType, newType, typeA, targetTypes);
                    if (typeA != typeB) {
                        BinaryPrimitives.WriteUInt64LittleEndian(
                            container.GetWritableTail((int)tailIndex),
                            genericMethod.Assembly.GetLocalTypeId(typeB)
                        );
                        changes++;
                    }
                    break;
            }
        }

        return changes == 0 ? genericMethod.GetIlContainer() : container;
    }

}
