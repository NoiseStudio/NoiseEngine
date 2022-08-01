using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class NeslTypeBuilder : NeslType {

    private readonly ConcurrentBag<NeslAttribute> customAttributes = new ConcurrentBag<NeslAttribute>();
    private readonly ConcurrentDictionary<string, NeslMethodBuilder> methods =
        new ConcurrentDictionary<string, NeslMethodBuilder>();

    public override IEnumerable<NeslAttribute> CustomAttributes => customAttributes;
    public override IEnumerable<NeslMethod> Methods => methods.Values;

    internal NeslTypeBuilder(NeslAssemblyBuilder assembly, string fullName, TypeAttributes attributes)
        : base(assembly, fullName, attributes) {
    }

    /// <summary>
    /// Creates new <see cref="NeslMethodBuilder"/> in this type.
    /// </summary>
    /// <param name="name">Name of new <see cref="NeslMethodBuilder"/>.</param>
    /// <param name="attributes"><see cref="MethodAttributes"/> of new method.</param>
    /// <param name="returnType"><see cref="NeslType"/> returned from new method.</param>
    /// <param name="parameterTypes"><see cref="NeslType"/> parameters of new method.</param>
    /// <returns>New <see cref="NeslMethodBuilder"/>.</returns>
    /// <exception cref="ArgumentException">
    /// <see cref="NeslMethod"/> with this <paramref name="name"/> already exists in this type.
    /// </exception>
    public NeslMethodBuilder DefineMethod(
        string name, MethodAttributes attributes = MethodAttributes.Public,
        NeslType? returnType = null, params NeslType[] parameterTypes
    ) {
        NeslMethodBuilder method = new NeslMethodBuilder(this, name, attributes, returnType, parameterTypes);

        if (!methods.TryAdd(name, method)) {
            throw new ArgumentException($"{nameof(NeslMethod)} named `{name}` already exists in `{Name}` type.",
                nameof(name));
        }

        return method;
    }

    /// <summary>
    /// Adds <paramref name="customAttribute"/> to this <see cref="NeslTypeBuilder"/>.
    /// </summary>
    /// <param name="customAttribute"><see cref="NeslAttribute"/>.</param>
    public void AddCustomAttribute(NeslAttribute customAttribute) {
        customAttributes.Add(customAttribute);
    }

}
