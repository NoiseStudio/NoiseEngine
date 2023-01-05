using NoiseEngine.Collections;
using NoiseEngine.Nesl.CompilerTools.Architectures.SpirV.Types;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.CompilerTools.Architectures.SpirV;

internal class SpirVBuiltInTypes {

    private readonly ConcurrentDictionary<object[], Lazy<SpirVType>> types =
        new ConcurrentDictionary<object[], Lazy<SpirVType>>(new ReadOnlyListEqualityComparer<object>());

    public SpirVCompiler Compiler { get; }

    public SpirVBuiltInTypes(SpirVCompiler compiler) {
        Compiler = compiler;
    }

    public bool TryGetTypeByName(NeslType neslType, string name, [NotNullWhen(true)] out SpirVType? type) {
        // Split name.
        List<string> args = new List<string>();

        int squareBrackets = 0;
        int index = 0;

        for (int i = 0; i < name.Length + 1; i++) {
            switch (i < name.Length ? name[i] : '`') {
                case '[':
                    squareBrackets++;
                    break;
                case ']':
                    squareBrackets--;
                    break;
                case '`':
                    if (squareBrackets != 0)
                        break;

                    if (name[index] == '[')
                        args.Add(name.Substring(index + 1, i - 2 - index));
                    else
                        args.Add(name.Substring(index, i - index));

                    index = i + 1;
                    break;
            }
        }

        // Find name.
        switch (args[0]) {
            case nameof(SpirVOpCode.OpTypeVoid):
                type = GetOpTypeVoid();
                return true;
            case nameof(SpirVOpCode.OpTypeInt):
                type = GetOpTypeInt(ulong.Parse(args[1]), uint.Parse(args[2]) == 1);
                return true;
            case nameof(SpirVOpCode.OpTypeFloat):
                type = GetOpTypeFloat(ulong.Parse(args[1]));
                return true;
            case nameof(SpirVOpCode.OpTypeVector):
                type = GetOpTypeVector(neslType.Assembly.GetType(Convert.ToUInt64(args[1], 16))!, uint.Parse(args[2]));
                return true;
            case "OpTypeArray":
                type = GetOpTypeRuntimeArray(neslType.Assembly.GetType(Convert.ToUInt64(args[1], 16))!);
                return true;
            default:
                type = null;
                return false;
        }
    }

    public SpirVType GetOpTypeVoid() {
        return types.GetOrAdd(new object[] { SpirVOpCode.OpTypeVoid }, _ => new Lazy<SpirVType>(() => {
            lock (Compiler.TypesAndVariables) {
                SpirVId id = Compiler.GetNextId();
                Compiler.TypesAndVariables.Emit(SpirVOpCode.OpTypeVoid, id);
                return new SpirVType(Compiler, id);
            }
        })).Value;
    }

    public SpirVType GetOpTypeInt(ulong size, bool signed) {
        return types.GetOrAdd(new object[] { SpirVOpCode.OpTypeInt, size, signed }, _ => new Lazy<SpirVType>(() => {
            lock (Compiler.TypesAndVariables) {
                SpirVId id = Compiler.GetNextId();
                Compiler.TypesAndVariables.Emit(
                    SpirVOpCode.OpTypeInt, id, ((uint)size).ToSpirVLiteral(), (signed ? 1u : 0u).ToSpirVLiteral()
                );
                return new SpirVType(Compiler, id);
            }
        })).Value;
    }

    public SpirVType GetOpTypeFloat(ulong size) {
        return types.GetOrAdd(new object[] { SpirVOpCode.OpTypeFloat, size }, _ => new Lazy<SpirVType>(() => {
            lock (Compiler.TypesAndVariables) {
                SpirVId id = Compiler.GetNextId();
                Compiler.TypesAndVariables.Emit(SpirVOpCode.OpTypeFloat, id, ((uint)size).ToSpirVLiteral());
                return new SpirVType(Compiler, id);
            }
        })).Value;
    }

    public SpirVType GetOpTypeVector(NeslType neslType, uint size) {
        return types.GetOrAdd(new object[] { SpirVOpCode.OpTypeVector, neslType, size },
            _ => new Lazy<SpirVType>(() => {
                lock (Compiler.TypesAndVariables) {
                    SpirVId id = Compiler.GetNextId();

                    Compiler.TypesAndVariables.Emit(
                        SpirVOpCode.OpTypeVector, id, Compiler.GetSpirVType(neslType).Id,
                        size.ToSpirVLiteral()
                    );

                    return new SpirVType(Compiler, id);
                }
            })).Value;
    }

    public SpirVType GetOpTypeRuntimeArray(NeslType elementType) {
        return types.GetOrAdd(new object[] { SpirVOpCode.OpTypeRuntimeArray, elementType },
            _ => new Lazy<SpirVType>(() => {
                SpirVType array;
                lock (Compiler.TypesAndVariables) {
                    SpirVId id = Compiler.GetNextId();

                    Compiler.TypesAndVariables.Emit(
                        SpirVOpCode.OpTypeRuntimeArray, id, Compiler.GetSpirVType(elementType).Id
                    );

                    array = new SpirVType(Compiler, id);
                }

                lock (Compiler.Annotations) {
                    Compiler.Annotations.Emit(
                        SpirVOpCode.OpDecorate, array.Id, (uint)Decoration.ArrayStride,
                        checked((uint)(elementType.GetSize() / 8)).ToSpirVLiteral()
                    );
                }

                return array;
        })).Value;
    }

    public SpirVType GetOpTypePointer(StorageClass storageClass, SpirVType type) {
        return types.GetOrAdd(new object[] { storageClass, type }, _ => new Lazy<SpirVType>(() => {
            lock (Compiler.TypesAndVariables) {
                SpirVId id = Compiler.GetNextId();
                Compiler.TypesAndVariables.Emit(SpirVOpCode.OpTypePointer, id, (uint)storageClass, type.Id);
                return new SpirVType(Compiler, id);
            }
        })).Value;
    }

    public SpirVType GetOpTypeFunction(SpirVType returnType, params SpirVType[] parameters) {
        object[] objects = new object[2 + parameters.Length];

        objects[0] = SpirVOpCode.OpTypeFunction;
        objects[1] = returnType;

        for (int i = 0; i < parameters.Length; i++)
            objects[2 + i] = parameters[i];

        return types.GetOrAdd(objects, _ => new Lazy<SpirVType>(() => {
            lock (Compiler.TypesAndVariables) {
                Span<SpirVId> parameterIds = stackalloc SpirVId[parameters.Length];
                for (int i = 0; i < parameterIds.Length; i++)
                    parameterIds[i] = parameters[i].Id;

                SpirVId id = Compiler.GetNextId();
                Compiler.TypesAndVariables.Emit(SpirVOpCode.OpTypeFunction, id, returnType.Id, parameterIds);
                return new SpirVType(Compiler, id);
            }
        })).Value;
    }

}
