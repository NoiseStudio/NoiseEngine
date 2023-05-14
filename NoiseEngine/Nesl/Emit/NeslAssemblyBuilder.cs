using NoiseEngine.Collections.Concurrent;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NoiseEngine.Nesl.Emit;

public class NeslAssemblyBuilder : NeslAssembly {

    private readonly Dictionary<ulong, NeslType> idToType = new Dictionary<ulong, NeslType>();
    private readonly Dictionary<NeslType, ulong> typeToId = new Dictionary<NeslType, ulong>();

    private readonly Dictionary<ulong, NeslMethod> idToMethod = new Dictionary<ulong, NeslMethod>();
    private readonly Dictionary<NeslMethod, ulong> methodToId = new Dictionary<NeslMethod, ulong>();

    private readonly ConcurrentHashSet<NeslAssembly> dependencies =
        new ConcurrentHashSet<NeslAssembly>();
    private readonly ConcurrentDictionary<string, NeslTypeBuilder> types =
        new ConcurrentDictionary<string, NeslTypeBuilder>();

    public override IEnumerable<NeslAssembly> Dependencies => dependencies;
    public override IEnumerable<NeslType> Types => types.Values;

    private NeslAssemblyBuilder(string name) : base(name) {
    }

    /// <summary>
    /// Creates new <see cref="NeslAssemblyBuilder"/>.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslAssemblyBuilder"/>.</param>
    /// <returns>New <see cref="NeslAssemblyBuilder"/>.</returns>
    public static NeslAssemblyBuilder DefineAssembly(string name) {
        NeslAssemblyBuilder builder = new NeslAssemblyBuilder(name);
        builder.dependencies.Add(Default.Manager.AssemblyBuilder);
        return builder;
    }

    internal static NeslAssemblyBuilder DefineAssemblyWithoutDefault(string name) {
        return new NeslAssemblyBuilder(name);
    }

    /// <summary>
    /// Tries to create new <see cref="NeslTypeBuilder"/> in this assembly.
    /// </summary>
    /// <param name="fullName">Name preceded by namespace.</param>
    /// <param name="typeBuilder">
    /// New <see cref="NeslTypeBuilder"/> or <see langword="null"/> when result is <see langword="false"/>.
    /// </param>
    /// <returns><see langword="true"/> when type is successfuly defined; otherwise <see langword="false"/>.</returns>
    public bool TryDefineType(string fullName, [NotNullWhen(true)] out NeslTypeBuilder? typeBuilder) {
        typeBuilder = new NeslTypeBuilder(this, fullName);

        if (!types.TryAdd(fullName, typeBuilder)) {
            typeBuilder = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates new <see cref="NeslTypeBuilder"/> in this assembly.
    /// </summary>
    /// <param name="fullName">Name preceded by namespace.</param>
    /// <returns>New <see cref="NeslTypeBuilder"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="NeslType"/> with this <paramref name="fullName"/> already exists in this assembly.
    /// </exception>
    public NeslTypeBuilder DefineType(string fullName) {
        if (TryDefineType(fullName, out NeslTypeBuilder? typeBuilder))
            return typeBuilder;
        throw new ArgumentException($"{nameof(NeslType)} named `{fullName}` already exists in `{Name}` assembly.",
            nameof(fullName));
    }

    internal override ulong GetLocalTypeId(NeslType type) {
        lock (idToType) {
            if (!typeToId.TryGetValue(type, out ulong id)) {
                id = (ulong)idToType.Count;
                idToType.Add(id, type);
                typeToId.Add(type, id);
            }

            return id;
        }
    }

    internal override ulong GetLocalMethodId(NeslMethod method) {
        lock (idToMethod) {
            if (!methodToId.TryGetValue(method, out ulong id)) {
                id = (ulong)idToMethod.Count;
                idToMethod.Add(id, method);
                methodToId.Add(method, id);
            }

            return id;
        }
    }

    internal override NeslType GetType(ulong localTypeId) {
        return idToType[localTypeId];
    }

    internal override NeslMethod GetMethod(ulong localMethodId) {
        return idToMethod[localMethodId];
    }

}
