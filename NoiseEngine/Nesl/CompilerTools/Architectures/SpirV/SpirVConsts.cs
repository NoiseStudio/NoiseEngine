using NoiseEngine.Collections;
using NoiseEngine.Nesl.Default;
using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVConsts {

    private readonly ConcurrentDictionary<(NeslType, EquatableReadOnlyList<byte>), Lazy<SpirVId>> constsComposite =
        new ConcurrentDictionary<(NeslType, EquatableReadOnlyList<byte>), Lazy<SpirVId>>();

    public SpirVCompiler Compiler { get; }

    public SpirVConsts(SpirVCompiler compiler) {
        Compiler = compiler;
    }

    public SpirVId GetConsts(NeslType type, IReadOnlyList<byte> data) {
        return constsComposite.GetOrAdd((type, new EquatableReadOnlyList<byte>(data)), _ => new Lazy<SpirVId>(() => {
            switch (type.FullNameWithAssembly) {
                case BuiltInTypes.UInt32Name:
                    return Compiler.GetConst(BinaryPrimitives.ReadUInt32LittleEndian(data.ToArray()));
                case BuiltInTypes.Int32Name:
                    return Compiler.GetConst(BinaryPrimitives.ReadInt32LittleEndian(data.ToArray()));
                case BuiltInTypes.Float32Name:
                    return Compiler.GetConst(BinaryPrimitives.ReadSingleLittleEndian(data.ToArray()));
                case Buffers.ReadWriteBufferName:
                    return GetConstsBuffer(type, data);
            }

            FastList<SpirVId> ids = new FastList<SpirVId>();
            ulong index = 0;
            foreach (NeslField field in type.Fields) {
                if (field.IsStatic)
                    continue;

                ulong size = field.FieldType.GetSize() / 8;
                ids.Add(GetConsts(field.FieldType, data.Skip((int)index).Take((int)size).ToArray()));
                index += size;
            }

            SpirVId id = Compiler.GetNextId();
            lock (Compiler.TypesAndVariables) {
                Compiler.TypesAndVariables.Emit(
                    SpirVOpCode.OpConstantComposite, Compiler.GetSpirVType(type).Id, id, ids.AsSpan()
                );
            }
            return id;
        })).Value;
    }

    private SpirVId GetConstsBuffer(NeslType type, IReadOnlyList<byte> data) {
        NeslType elementType = type.GenericMakedTypeParameters.Single();
        ulong elementSize = elementType.GetSize() / 8;
        uint length = (uint)((ulong)data.Count / elementSize);

        Span<SpirVId> ids = new SpirVId[length];
        ulong index = 0;
        for (int i = 0; i < length; i++) {
            ids[i] = GetConsts(elementType, data.Skip((int)index).Take((int)elementSize).ToArray());
            index += elementSize;
        }

        SpirVId id = Compiler.GetNextId();
        lock (Compiler.TypesAndVariables) {
            Compiler.TypesAndVariables.Emit(
                SpirVOpCode.OpConstantComposite, Compiler.GetSpirVType(type, length).Id, id, ids
            );
        }
        return id;
    }

}
