using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Serialization;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.CompilerTools.Generics;

internal static class GenericIlGenerator {

    public static IlContainer RemoveGenerics(
        NeslMethod genericMethod, IDictionary<NeslGenericTypeParameter, NeslType> targetTypes
    ) {
        SerializedIlContainer container = new SerializedIlContainer(
            genericMethod.GetIlContainer().GetRawInstructions(),
            genericMethod.GetIlContainer().GetTail(0).ToArray()
        );

        int changes = 0;
        foreach ((OpCode opCode, uint tailIndex) in container.GetRawInstructions()) {
            Instruction instruction = new Instruction(opCode, tailIndex, container);

            switch (opCode) {
                case OpCode.SetElement:
                    if (
                        genericMethod.Assembly.GetType(instruction.ReadUInt64()) is
                        NeslGenericTypeParameter genericTypeParameterA
                    ) {
                        BinaryPrimitives.WriteUInt64LittleEndian(
                            container.GetWritableTail((int)tailIndex),
                            genericMethod.Assembly.GetLocalTypeId(targetTypes[genericTypeParameterA])
                        );
                    }

                    changes++;
                    break;
            }
        }

        return changes == 0 ? genericMethod.GetIlContainer() : container;
    }

}
