using NoiseEngine.Nesl.Serialization;
using NoiseEngine.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

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
    /// <param name="dependencies">Dependencies of loading <see cref="NeslAssembly"/>.</param>
    /// <returns>Loaded <see cref="NeslAssembly"/>.</returns>
    public static NeslAssembly Load(byte[] rawBytes, IEnumerable<NeslAssembly> dependencies) {
        SerializationReader reader = new SerializationReader(rawBytes);
        if (reader.ReadUInt32() != 0)
            throw new ArgumentException("Unsupported serialization version.", nameof(rawBytes));

        string name = reader.ReadString();
        List<NeslAssembly> finalDependencies = new List<NeslAssembly>();
        foreach (string dependencyName in reader.ReadEnumerableString())
            finalDependencies.Add(dependencies.First(x => x.Name == dependencyName));
        SerializedNeslAssembly assembly = new SerializedNeslAssembly(name, finalDependencies);

        reader.AddToStorage(typeof(NeslAssembly), assembly);

        ulong length = (ulong)reader.ReadInt32();
        for (ulong i = 0; i < length; i++) {
            NeslType type;
            if (reader.ReadBool()) {
                NeslTypeUsageKind usage = (NeslTypeUsageKind)reader.ReadUInt8();
                type = usage switch {
                    NeslTypeUsageKind.Normal => new SerializedNeslType(
                        assembly, reader.ReadString(), reader.ReadEnumerable<NeslAttribute>().ToArray()
                    ),
                    NeslTypeUsageKind.GenericTypeParameter => new SerializedNeslGenericTypeParameter(
                        reader.ReadBool() ?
                            assembly.GetType(reader.ReadUInt64()) : assembly.GetMethod(reader.ReadUInt64()),
                        reader.ReadString(), reader.ReadEnumerable<NeslAttribute>().ToArray()
                    ),
                    NeslTypeUsageKind.GenericMaked => throw new NotImplementedException(),
                    _ => throw new NotImplementedException(),
                };
            } else {
                string temp = reader.ReadString();
                type = dependencies.First(x => x.Name == temp).GetType(reader.ReadString())
                    ?? throw new NullReferenceException();
            }

            assembly.typeToId[type] = i;
            assembly.idToType[i] = type;
        }

        int l = reader.ReadInt32();
        for (int i = 0; i < l; i++) {
            ulong id = reader.ReadUInt64();
            NeslType type = assembly.GetType(id).MakeGeneric(
                reader.ReadEnumerableUInt64().Select(assembly.GetType).ToArray()
            );

            assembly.typeToId[type] = id;
            assembly.idToType[id] = type;
        }

        l = reader.ReadInt32();
        NeslType[] types = new NeslType[l];
        for (int i = 0; i < l; i++) {
            SerializedNeslType type = (SerializedNeslType)assembly.GetType(reader.ReadUInt64());
            type.DeserializeBody(reader);
            types[i] = type;
        }
        assembly.SetTypes(types);

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
        typeWriter.WriteInt32(Types.Count());
        foreach (NeslType type in Types) {
            typeWriter.WriteUInt64(GetLocalTypeId(type));
            type.SerializeBody(typeWriter);
        }

        typeWriter.WriteInt32(idToMethod.Count);
        foreach (NeslMethod method in idToMethod.Values) {
            typeWriter.WriteUInt64(GetLocalTypeId(method.Type));
            typeWriter.WriteString(method.Name);
            typeWriter.WriteEnumerable(method.GenericTypeParameters.Select(GetLocalTypeId));
            typeWriter.WriteEnumerable(method.ParameterTypes.Select(GetLocalTypeId));
        }

        List<NeslType> genericMakedTypes = new List<NeslType>();

        writer.WriteInt32(idToType.Count);
        foreach (NeslType type in idToType.Values) {
            if (type.SerializeHeader(this, writer))
                genericMakedTypes.Add(type);
        }

        writer.WriteInt32(genericMakedTypes.Count);
        foreach (NeslType type in genericMakedTypes) {
            writer.WriteUInt64(GetLocalTypeId(type));
            writer.WriteEnumerable(type.GenericMakedTypeParameters.Select(GetLocalTypeId));
        }

        writer.WriteBytes(typeWriter.AsSpan());

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

}
