using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class NeslTypeBuilder : NeslType {

    private readonly ConcurrentDictionary<string, NeslMethodBuilder> methods =
        new ConcurrentDictionary<string, NeslMethodBuilder>();

    public override IEnumerable<NeslMethod> Methods => methods.Values;

    internal NeslTypeBuilder(string fullName) : base(fullName) {
    }

    /// <summary>
    /// Creates new <see cref="NeslMethodBuilder"/> in this type.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslMethodBuilder"/>.</param>
    /// <returns>New <see cref="NeslMethodBuilder"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="NeslMethod"/> with this <paramref name="name"/> already exists in this type.
    /// </exception>
    public NeslMethodBuilder DefineType(string name) {
        NeslMethodBuilder method = new NeslMethodBuilder(name);

        if (!methods.TryAdd(name, method)) {
            throw new ArgumentException($"{nameof(NeslMethod)} named `{name}` already exists in `{Name}` type.",
                nameof(name));
        }

        return method;
    }

}
