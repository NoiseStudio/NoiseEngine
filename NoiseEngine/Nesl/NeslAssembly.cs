using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NoiseEngine.Nesl;

public abstract class NeslAssembly {

    private readonly Dictionary<ulong, NeslType> idToType = new Dictionary<ulong, NeslType>();
    private readonly Dictionary<NeslType, ulong> typeToId = new Dictionary<NeslType, ulong>();

    private readonly Dictionary<ulong, NeslMethod> idToMethod = new Dictionary<ulong, NeslMethod>();
    private readonly Dictionary<NeslMethod, ulong> methodToId = new Dictionary<NeslMethod, ulong>();

    public abstract IEnumerable<NeslAssembly> Dependencies { get; }
    public abstract IEnumerable<NeslType> Types { get; }

    public string Name { get; }

    protected NeslAssembly(string name) {
        Name = name;
    }

    /// <summary>
    /// Loads a <see cref="NeslAssembly"/> from <paramref name="rawBytes"/>.
    /// </summary>
    /// <param name="rawBytes">Raw <see cref="byte"/>s of <see cref="NeslAssembly"/>.</param>
    /// <param name="dependencies">
    /// Dependencies of loading <see cref="NeslAssembly"/>. Regardless of the state, it always contains the default
    /// library.
    /// </param>
    /// <returns>Loaded <see cref="NeslAssembly"/>.</returns>
    public static NeslAssembly Load(byte[] rawBytes, IEnumerable<NeslAssembly>? dependencies = null) {
        return LoadWithoutDefault(rawBytes,
            dependencies is null ? new NeslAssembly[] { Manager.Assembly } : dependencies.Prepend(Manager.Assembly)
        );
    }

    /// <summary>
    /// Loads a <see cref="NeslAssembly"/> from <paramref name="rawBytes"/>.
    /// </summary>
    /// <param name="rawBytes">Raw <see cref="byte"/>s of <see cref="NeslAssembly"/>.</param>
    /// <param name="dependencies">Dependencies of loading <see cref="NeslAssembly"/>.</param>
    /// <returns>Loaded <see cref="NeslAssembly"/>.</returns>
    internal static NeslAssembly LoadWithoutDefault(byte[] rawBytes, IEnumerable<NeslAssembly>? dependencies = null) {
        SerializationReader reader = new SerializationReader(rawBytes);
        if (reader.ReadUInt32() != 0)
            throw new ArgumentException("Unsupported serialization version.", nameof(rawBytes));

        string name = reader.ReadString();
        List<NeslAssembly> finalDependencies = new List<NeslAssembly>();
        foreach (string dependencyName in reader.ReadEnumerableString()) {
            NeslAssembly? dependency = dependencies?.FirstOrDefault(x => x.Name == dependencyName);
            if (dependency is null)
                throw new ArgumentException($"Cannot find dependency {dependencyName}.", nameof(rawBytes));
            finalDependencies.Add(dependency);
        }
        dependencies = null;

        SerializedNeslAssembly assembly = new SerializedNeslAssembly(name, finalDependencies);

        reader.AddToStorage(typeof(NeslAssembly), assembly);

        ulong length = (ulong)reader.ReadInt32();
        List<NeslType> types = new List<NeslType>();
        for (ulong i = 0; i < length; i++) {
            ulong id = reader.ReadUInt64();

            NeslType type;
            bool own = reader.ReadBool();
            NeslTypeUsageKind usage = default;
            if (own) {
                usage = (NeslTypeUsageKind)reader.ReadUInt8();
                type = usage switch {
                    NeslTypeUsageKind.Normal => new SerializedNeslType(assembly, reader),
                    NeslTypeUsageKind.GenericTypeParameter => new SerializedNeslGenericTypeParameter(
                        assembly, reader.ReadString(), reader.ReadEnumerable<NeslAttribute>().ToArray()
                    ),
                    NeslTypeUsageKind.GenericMaked => assembly.DeserializeGenericMaked(reader),
                    NeslTypeUsageKind.GenericNotFullyConstructed =>
                        assembly.DeserializeGenericNotFullyConstructed(reader),
                    _ => throw new NotImplementedException(),
                };
            } else {
                string temp = reader.ReadString();
                type = dependencies?.First(x => x.Name == temp).GetType(reader.ReadString())
                    ?? throw new NullReferenceException();
            }

            assembly.typeToId[type] = id;
            assembly.idToType[id] = type;

            if (own && usage == NeslTypeUsageKind.Normal && type is SerializedNeslType serialized) {
                serialized.DeserializeBody(reader);
                types.Add(type);
            }
        }
        assembly.SetTypes(types.ToArray());

        int l = reader.ReadInt32();
        for (int i = 0; i < l; i++) {
            ulong id = reader.ReadUInt64();
            NeslType type = assembly.GetType(id);
            if (type.IsGeneric) {
                type = type.MakeGeneric(
                    reader.ReadEnumerableUInt64().Select(assembly.GetType).ToArray()
                );
            } else {
                SerializedNeslType serialized = (SerializedNeslType)type;
                serialized.UnsafeInitializeTypeFromMakeGeneric(serialized.UnsafeTargetTypesFromMakeGeneric(
                    serialized.GenericMakedFrom!.GenericTypeParameters,
                    serialized.GenericMakedTypeParameters.ToArray()
                ) ?? throw new UnreachableException());
            }

            assembly.typeToId[type] = id;
            assembly.idToType[id] = type;
        }

        length = (ulong)reader.ReadInt32();
        for (ulong i = 0; i < length; i++) {
            NeslType type = assembly.GetType(reader.ReadUInt64());
            name = reader.ReadString();
            NeslGenericTypeParameter[] genericTypeParameters = reader.ReadEnumerableUInt64().Select(assembly.GetType)
                .Cast<NeslGenericTypeParameter>().ToArray();
            NeslType[] parameterTypes = reader.ReadEnumerableUInt64().Select(assembly.GetType).ToArray();
            NeslMethod method = type.Methods.First(
                x => x.Name == name && x.GenericTypeParameters.SequenceEqual(genericTypeParameters) &&
                x.ParameterTypes.SequenceEqual(parameterTypes)
            );

            assembly.methodToId[method] = i;
            assembly.idToMethod[i] = method;
        }

        return assembly;
    }

    /// <summary>
    /// Serializes this <see cref="NeslAssembly"/> to raw bytes.
    /// </summary>
    /// <returns>Raw bytes of this <see cref="NeslAssembly"/>.</returns>
    public byte[] GetRawBytes() {
        SerializationWriter writer = new SerializationWriter();
        writer.WriteUInt32(0); // Serialization version.
        writer.WriteString(Name);
        writer.WriteEnumerable(Dependencies.Select(x => x.Name));

        SerializationWriter typeWriter = new SerializationWriter();

        SerializationUsed used = new SerializationUsed();
        Dictionary<NeslType, (int, int)> typeWriters = new Dictionary<NeslType, (int, int)>();
        foreach (NeslType type in Types) {
            int start = typeWriter.Count;
            type.SerializeBody(used, typeWriter);
            typeWriters[type] = (start, typeWriter.Count - start);
        }

        int methodStart = typeWriter.Count;
        typeWriter.WriteInt32(idToMethod.Count);
        foreach (NeslMethod method in idToMethod.Values) {
            typeWriter.WriteString(method.Name);
            typeWriter.WriteEnumerable(method.GenericTypeParameters.Select(GetLocalTypeId));
            typeWriter.WriteEnumerable(method.ParameterTypes.Select(GetLocalTypeId));
        }

        foreach (NeslType type in idToType.Values)
            type.PrepareHeader(used, this);

        List<(NeslType, bool)> genericMakedTypes = new List<(NeslType, bool)>();

        int startCount = writer.Count;
        writer.WriteInt32(-1);

        int i = 0;
        foreach (NeslType type in used.OrderedTypes.Concat(idToType.Values.Except(used.Types)).Distinct().ToArray()) {
            writer.WriteUInt64(GetLocalTypeId(type));
            if (type.SerializeHeader(this, writer))
                genericMakedTypes.Add((type, type.Assembly == this));

            if (typeWriters.TryGetValue(type, out (int, int) o))
                writer.WriteBytes(typeWriter.AsSpan(o.Item1, o.Item2));
            i++;
        }

        Span<byte> span = writer.AsSpan(startCount);
        if (writer.IsLittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(span, i);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, i);

        writer.WriteInt32(genericMakedTypes.Count);
        foreach ((NeslType type, bool isLocal) in genericMakedTypes) {
            writer.WriteUInt64(GetLocalTypeId(type));
            if (!isLocal)
                writer.WriteEnumerable(type.GenericMakedTypeParameters.Select(GetLocalTypeId));
        }

        writer.WriteBytes(typeWriter.AsSpan(methodStart));

        return writer.ToArray();
    }

    /// <summary>
    /// Finds <see cref="NeslType"/> with given <paramref name="fullName"/> in this
    /// <see cref="NeslAssembly"/> and their dependencies.
    /// </summary>
    /// <param name="fullName">Full name of the searched <see cref="NeslType"/>.</param>
    /// <returns><see cref="NeslType"/> when type was found, <see langword="null"/> when not.</returns>
    public NeslType? GetType(string fullName) {
        return GetType(fullName.AsSpan());
    }

    private NeslType? GetType(ReadOnlySpan<char> fullName) {
        int index = fullName.IndexOf("::");
        if (index == -1) {
            NeslType? type = GetTypeLocal(fullName);
            if (type is not null)
                return type;

            foreach (NeslAssembly dependency in Dependencies) {
                type = dependency.GetTypeLocal(fullName);
                if (type is not null)
                    return type;
            }

            return null;
        }

        ReadOnlySpan<char> assemblyName = fullName[..index];
        ReadOnlySpan<char> fullNameWithoutAssembly = fullName[(index + 2)..];
        if (assemblyName.SequenceEqual(Name))
            return GetTypeLocal(fullNameWithoutAssembly);

        foreach (NeslAssembly dependency in Dependencies) {
            if (assemblyName.SequenceEqual(dependency.Name))
                return dependency.GetTypeLocal(fullNameWithoutAssembly);
        }

        return null;
    }

    private NeslType? GetTypeLocal(ReadOnlySpan<char> fullNameWithoutAssembly) {
        foreach (NeslType type in Types) {
            if (fullNameWithoutAssembly.SequenceEqual(type.FullName))
                return type;
        }

        return null;
    }

    internal ulong GetLocalTypeId(NeslType type) {
        lock (idToType) {
            if (!typeToId.TryGetValue(type, out ulong id)) {
                id = (ulong)idToType.Count;
                idToType.Add(id, type);
                typeToId.Add(type, id);
            }

            return id;
        }
    }

    internal ulong GetLocalMethodId(NeslMethod method) {
        lock (idToMethod) {
            if (!methodToId.TryGetValue(method, out ulong id)) {
                id = (ulong)idToMethod.Count;
                idToMethod.Add(id, method);
                methodToId.Add(method, id);
            }

            return id;
        }
    }

    internal NeslType GetType(ulong localTypeId) {
        return idToType[localTypeId];
    }

    internal NeslMethod GetMethod(ulong localMethodId) {
        return idToMethod[localMethodId];
    }

    private NeslType DeserializeGenericMaked(SerializationReader reader) {
        NeslType parent = GetType(reader.ReadUInt64());
        SerializedNeslType result = parent.UnsafeCreateTypeFromMakeGeneric(
            reader.ReadEnumerableUInt64().Select(GetType).ToArray()
        );
        parent.UnsafeAddToGenericMaked(result);
        return result;
    }

    private NeslType DeserializeGenericNotFullyConstructed(SerializationReader reader) {
        NeslType parent = GetType(reader.ReadUInt64());
        return parent.MakeGeneric(reader.ReadEnumerableUInt64().Select(GetType).ToArray());
    }

}
