using NoiseEngine.Nesl.Emit;
using NoiseEngine.Nesl.Emit.Attributes;
using NoiseEngine.Nesl.Emit.Attributes.Internal;
using System.Runtime.InteropServices;

namespace NoiseEngine.Nesl.Default;

internal static class Buffers {

    private static readonly NeslType readWriteBuffer;

    static Buffers() {
        readWriteBuffer = CreateReadWriteBuffer();
    }

    public static NeslType GetReadWriteBuffer(NeslType type) {
        return readWriteBuffer.MakeGeneric(type);
    }

    private static NeslType CreateReadWriteBuffer() {
        NeslTypeBuilder type = Manager.AssemblyBuilder.DefineType($"{Manager.AssemblyBuilder.Name}.ReadWriteBuffer`1");
        NeslGenericTypeParameterBuilder genericTypeParameter = type.DefineGenericTypeParameter("T");
        type.AddAttribute(PlatformDependentTypeRepresentationAttribute.Create(
            $"OpTypeArray`{{{genericTypeParameter.Name}}}"
        ));
        type.AddAttribute(SizeAttribute.Create((ulong)Marshal.SizeOf<nuint>() * 8));

        // Indexer get.
        NeslMethodBuilder indexerGet = type.DefineMethod(
            NeslOperators.IndexerGet, genericTypeParameter, BuiltInTypes.UInt32
        );
        indexerGet.AddAttribute(IntrinsicAttribute.Create());

        // Indexer set.
        NeslMethodBuilder indexerSet = type.DefineMethod(
            NeslOperators.IndexerSet, null, BuiltInTypes.UInt32, genericTypeParameter
        );
        indexerSet.AddAttribute(IntrinsicAttribute.Create());

        return type;
    }

}
