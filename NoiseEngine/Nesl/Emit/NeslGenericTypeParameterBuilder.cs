using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace NoiseEngine.Nesl.Emit;

public class NeslGenericTypeParameterBuilder : NeslGenericTypeParameter {

    private readonly ConcurrentBag<NeslAttribute> attributes = new ConcurrentBag<NeslAttribute>();

    public override IEnumerable<NeslAttribute> Attributes => attributes;

    internal NeslGenericTypeParameterBuilder(NeslAssembly assembly, string name) : base(assembly, name) {
    }

    /// <summary>
    /// Adds <paramref name="attribute"/> to this <see cref="NeslGenericTypeParameterBuilder"/>.
    /// </summary>
    /// <param name="attribute"><see cref="NeslAttribute"/>.</param>
    /// <exception cref="InvalidOperationException">
    /// Given <paramref name="attribute"/> cannot be assigned to this target.
    /// </exception>
    public void AddAttribute(NeslAttribute attribute) {
        if (!attribute.Targets.HasFlag(AttributeTargets.GenericTypeParameter)) {
            throw new InvalidOperationException(
                $"The `{attribute}` attribute cannot be assigned to a generic type parameter.");
        }

        attributes.Add(attribute);
    }

}
