using NoiseEngine.Nesl.Default;
using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

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
                        assembly, reader.ReadString(), reader.ReadEnumerable<NeslAttribute>().ToArray(),
                        reader.ReadEnumerableUInt64().Select(assembly.GetType).ToArray()
                    ),
                    NeslTypeUsageKind.GenericMaked => assembly.DeserializeGenericMaked(reader),
                    NeslTypeUsageKind.GenericNotFullyConstructed =>
                        assembly.DeserializeGenericNotFullyConstructed(reader),
                    _ => throw new NotImplementedException(),
                };
            } else {
                string temp = reader.ReadString();
                NeslAssembly? dependency = finalDependencies.Find(x => x.Name == temp);
                if (dependency is null)
                    throw new InvalidOperationException($"Unable to find NESL dependency named `{temp}`.");

                temp = reader.ReadString();
                NeslType? tempType = dependency.GetType(temp);
                if (tempType is null)
                    throw new InvalidOperationException($"Unable to find NESL type named `{temp}`.");
                type = tempType;

                if (reader.ReadBool()) {
                    type = type.MakeGeneric(
                        reader.ReadEnumerableUInt64().Select(assembly.GetType).ToArray()
                    );
                }
            }

            assembly.typeToId[type] = id;
            assembly.idToType[id] = type;

            if (own && usage == NeslTypeUsageKind.Normal && type is SerializedNeslType serialized) {
                serialized.DeserializeBody(reader);
                types.Add(type);
            }
        }
        assembly.SetTypes(types.ToArray());

        // Deserialize methods.
        int l = reader.ReadInt32();
        for (int i = 0; i < l; i++) {
            SerializedNeslType type = (SerializedNeslType)assembly.GetType(reader.ReadUInt64());
            type.DeserializeMethods(reader);
        }

        // Create generics.
        l = reader.ReadInt32();
        for (int i = 0; i < l; i++) {
            ulong id = reader.ReadUInt64();

            SerializedNeslType type = (SerializedNeslType)assembly.GetType(id);
            type.SetGenericMakedTypeParameters(reader.ReadEnumerableUInt64().Select(assembly.GetType).ToArray());
            type.GenericMakedFrom!.UnsafeAddToGenericMaked(type);

            type.UnsafeInitializeTypeFromMakeGeneric(type.UnsafeTargetTypesFromMakeGeneric(
                type.GenericMakedFrom!.GenericTypeParameters,
                type.GenericMakedTypeParameters.ToArray(),
                out bool isFullyConstructed
            ));
            Debug.Assert(isFullyConstructed);

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
        Dictionary<NeslType, (int, int)> methodsWriters = new Dictionary<NeslType, (int, int)>();
        foreach (NeslType type in Types) {
            int start = typeWriter.Count;
            type.SerializeBody(used, typeWriter);
            typeWriters[type] = (start, typeWriter.Count - start);

            start = typeWriter.Count;
            type.SerializeMethods(typeWriter);
            methodsWriters[type] = (start, typeWriter.Count - start);
        }

        int methodStart = typeWriter.Count;
        typeWriter.WriteInt32(idToMethod.Count);
        foreach (NeslMethod method in idToMethod.Values) {
            typeWriter.WriteUInt64(GetLocalTypeId(method.Type));
            typeWriter.WriteString(method.Name);
            typeWriter.WriteEnumerable(method.GenericTypeParameters.Select(GetLocalTypeId));
            typeWriter.WriteEnumerable(method.ParameterTypes.Select(GetLocalTypeId));
        }

        foreach (NeslType type in idToType.Values)
            type.PrepareHeader(used, this);

        List<NeslType> genericMakedTypes = new List<NeslType>();

        // Write type headers.
        int startCount = writer.Count;
        writer.WriteInt32(-1);

        int i = 0;
        foreach (NeslType type in used.OrderedTypes.Concat(idToType.Values.Except(used.Types)).Distinct().ToArray()) {
            writer.WriteUInt64(GetLocalTypeId(type));
            if (type.SerializeHeader(this, writer))
                genericMakedTypes.Add(type);

            if (typeWriters.TryGetValue(type, out (int, int) o))
                writer.WriteBytes(typeWriter.AsSpan(o.Item1, o.Item2));
            i++;
        }

        Span<byte> span = writer.AsSpan(startCount);
        if (writer.IsLittleEndian)
            BinaryPrimitives.WriteInt32LittleEndian(span, i);
        else
            BinaryPrimitives.WriteInt32BigEndian(span, i);

        // Write methods.
        writer.WriteInt32(methodsWriters.Count);
        foreach ((NeslType type, (int start, int length)) in methodsWriters) {
            writer.WriteUInt64(GetLocalTypeId(type));
            writer.WriteBytes(typeWriter.AsSpan(start, length));
        }

        // Write generic types.
        writer.WriteInt32(genericMakedTypes.Count);
        foreach (NeslType type in genericMakedTypes) {
            writer.WriteUInt64(GetLocalTypeId(type));
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
        return parent.UnsafeCreateTypeFromMakeGeneric(Array.Empty<NeslType>());
    }

    private NeslType DeserializeGenericNotFullyConstructed(SerializationReader reader) {
        NeslType parent = GetType(reader.ReadUInt64());
        return parent.MakeGeneric(reader.ReadEnumerableUInt64().Select(GetType).ToArray());
    }

}
