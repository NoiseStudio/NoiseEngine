using System;
using System.Collections.Generic;

namespace NoiseEngine.Nesl;

public abstract class NeslAssembly {

    public abstract IEnumerable<NeslAssembly> Dependencies { get; }
    public abstract IEnumerable<NeslType> Types { get; }

    public string Name { get; }

    protected NeslAssembly(string name) {
        Name = name;
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

    internal abstract ulong GetLocalTypeId(NeslType type);
    internal abstract ulong GetLocalMethodId(NeslMethod method);
    internal abstract NeslType GetType(ulong localTypeId);
    internal abstract NeslMethod GetMethod(ulong localMethodId);

}
